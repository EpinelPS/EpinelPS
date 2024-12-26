using EpinelPS.Database;
using EpinelPS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Controllers
{
    [Route("admin/Users")]
    public class UsersController(ILogger<AdminController> logger) : Controller
    {
        private readonly ILogger<AdminController> _logger = logger;
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

            var user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Route("SetPassword/{id}")]
        public IActionResult SetPassword(ulong id)
        {
            if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

            var user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
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

            var userToUpdate = JsonDb.Instance.Users.Where(s => s.ID == id).FirstOrDefault();
            if (userToUpdate == null)
            {
                return NotFound();
            }

            userToUpdate.Password = Convert.ToHexString(sha.ComputeHash(Encoding.ASCII.GetBytes(newPw))).ToLower(); ;

            return View(userToUpdate);
        }
    }
}
