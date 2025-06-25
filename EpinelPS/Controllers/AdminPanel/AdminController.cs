using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Security;
using System.Linq;

namespace EpinelPS.Controllers.AdminPanel
{
    [Route("admin")]
    public class AdminController(ILogger<AdminController> logger) : Controller
    {
        private readonly ILogger<AdminController> _logger = logger;

        public static bool CheckAuth(HttpContext context)
        {
            string? token = context.Request.Cookies["token"];
            if (token == null)
            {
                token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            }

            // TODO better authentication
            if (JsonDb.Instance.AdminAuthTokens.ContainsKey(token))
            {
                ulong userId = JsonDb.Instance.AdminAuthTokens[token];
                var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
                if (user != null && user.IsAdmin)
                    return true;
            }
            return false;
        }

        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            if (!CheckAuth(HttpContext)) return Redirect("/admin/");

            return View();
        }
        [Route("Events")]
        public IActionResult Events()
        {
            if (!CheckAuth(HttpContext)) return Redirect("/admin/");

            return View();
        }

        [Route("Configuration")]
        public IActionResult Configuration()
        {
            if (!CheckAuth(HttpContext)) return Redirect("/admin/");

            ServerConfiguration model = new()
            {
                LogType = JsonDb.Instance.LogLevel
            };

            return View(model);
        }

        [Route("Configuration"), ActionName("Configuration")]
        [HttpPost]
        public IActionResult ConfigurationSave([FromForm] ServerConfiguration cfg)
        {
            if (!CheckAuth(HttpContext)) return Redirect("/admin/");

            if (!ModelState.IsValid)
                return View();

            JsonDb.Instance.LogLevel = cfg.LogType;
            JsonDb.Save();

            return View(new ServerConfiguration() { LogType = cfg.LogType });
        }

        [Route("Mail")]
        public IActionResult Mail()
        {
            if (!CheckAuth(HttpContext)) return Redirect("/admin/");

            return View();
        }
        [Route("Database")]
        public IActionResult Database()
        {
            if (!CheckAuth(HttpContext)) return Redirect("/admin/");

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
