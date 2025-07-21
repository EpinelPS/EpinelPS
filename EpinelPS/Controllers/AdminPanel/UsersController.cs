using EpinelPS.Database;
using EpinelPS.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Controllers.AdminPanel
{
    [Route("admin/Users")]
    public class UsersController(ILogger<UsersController> logger) : Controller
    {
        private readonly ILogger<UsersController> _logger = logger;
        private static readonly MD5 sha = MD5.Create();

        public IActionResult Index()
        {
            if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

            return View(JsonDb.Instance.Users);
        }

        [Route("Modify/{id}")]
        public IActionResult Modify(ulong id)
        {
            if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

            User? user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }

            return View(
                new ModUserModel()
                {
                    IsAdmin = user.IsAdmin,
                    IsBanned = user.IsBanned,
                    Nickname = user.Nickname ?? "Unknown nickname",
                    sickpulls = user.sickpulls,
                    Username = user.Username ?? "Unknown username",
                    ID = user.ID
                }
            );
        }

        [Route("Modify/{id}"), ActionName("Modify")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoModifyUser(ulong id, [FromForm] ModUserModel toSet)
        {
            if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

            if (!ModelState.IsValid) throw new Exception("model state invalid");

            User? user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(toSet.Username))
                throw new Exception("username cannot be empty");

            user.Username = toSet.Username;
            user.IsAdmin = toSet.IsAdmin;
            user.sickpulls = toSet.sickpulls;
            user.IsBanned = toSet.IsBanned;
            user.Nickname = toSet.Nickname;
            JsonDb.Save();

            return View(new ModUserModel()
                {
                    IsAdmin = user.IsAdmin,
                    IsBanned = user.IsBanned,
                    Nickname = user.Nickname,
                    sickpulls = user.sickpulls,
                    Username = user.Username,
                    ID = user.ID
                });
        }

        [Route("SetPassword/{id}")]
        public IActionResult SetPassword(ulong id)
        {
            if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

            User? user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }

            user.Password = ""; // do not return the password

            return View(user);
        }


        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Route("SetPassword")]
        [HttpPost, ActionName("SetPassword")]
        [ValidateAntiForgeryToken]
        public IActionResult SetPasswordConfirm(ulong? id)
        {
            if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

            if (id == null)
            {
                return NotFound();
            }

            string? newPw = Request.Form["PasswordHash"];
            if (string.IsNullOrEmpty(newPw))
            {
                return BadRequest();
            }

            // TODO: use bcrypt

            User? userToUpdate = JsonDb.Instance.Users.Where(s => s.ID == id).FirstOrDefault();
            if (userToUpdate == null)
            {
                return NotFound();
            }

            userToUpdate.Password = Convert.ToHexString(sha.ComputeHash(Encoding.ASCII.GetBytes(newPw))).ToLower();

            return View(userToUpdate);
        }
    }
}
