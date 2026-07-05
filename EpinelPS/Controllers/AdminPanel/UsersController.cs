using EpinelPS.Database;
using EpinelPS.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Controllers.AdminPanel;

[Route("admin/Users")]
public class UsersController(ILogger<UsersController> logger, GameContext dbContext) : Controller
{
    private readonly ILogger<UsersController> _logger = logger;
    private readonly GameContext _db = dbContext;
    private static readonly MD5 sha = MD5.Create();

    public IActionResult Index()
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        return View(_db.SdkUsers);
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

        var sdkUser = _db.SdkUsers.Find(id);
        if (sdkUser == null) return NotFound();

        var gameUser = _db.Users.Find(id);
        if (gameUser == null) return NotFound();

        return View(
            new ModUserModel()
            {
                IsAdmin = sdkUser.IsAdmin,
                IsBanned = gameUser.IsBanned,
                Nickname = gameUser.Nickname ?? "Unknown nickname",
                sickpulls = gameUser.sickpulls,
                Username = sdkUser.Email ?? "Unknown username",
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

        if (string.IsNullOrEmpty(toSet.Username))
            throw new Exception("username cannot be empty");

        var sdkUser = _db.SdkUsers.Find(id);
        if (sdkUser == null) return NotFound();
        var gameUser = _db.Users.Find(id);
        if (gameUser == null) return NotFound();
        sdkUser.Email = toSet.Username;
        sdkUser.IsAdmin = toSet.IsAdmin;
        gameUser.Nickname = toSet.Nickname;
        gameUser.sickpulls = toSet.sickpulls;
        gameUser.IsBanned = toSet.IsBanned;
        _db.SaveChanges();

        return View(new ModUserModel()
        {
            IsAdmin = sdkUser.IsAdmin,
            IsBanned = gameUser.IsBanned,
            Nickname = gameUser.Nickname,
            sickpulls = gameUser.sickpulls,
            Username = sdkUser.Email,
            ID = id
        });
    }

    [Route("Currency/{id}")]
    public IActionResult Currency(ulong id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        User? user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
        if (user == null)
        {
            return NotFound();
        }

        return View(
            new ModUserCurrencyModel()
            {
                ID = user.ID,
                Current = user.Currency
            }
        );
    }

    [Route("Currency/{id}"), ActionName("Currency")]
    [HttpPost]
    public IActionResult CurrencyModify(ulong id, [FromForm] ModUserCurrencyModel model)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        User? user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
        if (user == null)
        {
            return NotFound();
        }

        user.AddCurrency(model.ToModify, model.Amount);
        JsonDb.Save();

        return View(
            new ModUserCurrencyModel()
            {
                ID = user.ID,
                Current = user.Currency
            }
        );
    }

    [Route("SetPassword/{id}")]
    public IActionResult SetPassword(ulong id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        SdkUser? user = _db.SdkUsers.Find(id);
        if (user == null) return NotFound();

        return View(new ChangeUserPasswordModel()
        {
            Email = user.Email,
            ID = user.ID
        });
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
        SdkUser? user = _db.SdkUsers.Find(id);
        if (user == null) return NotFound();
        user.PasswordHash = Convert.ToHexString(sha.ComputeHash(Encoding.ASCII.GetBytes(newPw))).ToLower();
        _db.SaveChanges();

        return View(new ChangeUserPasswordModel()
        {
            Email = user.Email,
            ID = user.ID
        });
    }
    [Route("GetUsersList")]
    public IActionResult GetUsersList()
    {
        if (!AdminController.CheckAuth(HttpContext)) return Unauthorized();
        var users = _db.Users
            .Select(u => new { u.ID, u.Nickname })
            .ToList();
        return Ok(users);
    }
}
