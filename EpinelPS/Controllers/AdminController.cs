using EpinelPS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EpinelPS.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        private bool CheckAuth()
        {
            string? token = HttpContext.Request.Cookies["token"];
            if (token == null) return false;


            foreach (var item in AdminApiController.AdminAuthTokens)
            {
                if (item.Key == token) return true;
            }
            return false;
        }
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            if (!CheckAuth()) return Redirect("/admin/");

            return View();
        }
        [Route("Events")]
        public IActionResult Events()
        {
            if (!CheckAuth()) return Redirect("/admin/");

            return View();
        }
        [Route("Configuration")]
        public IActionResult Configuration()
        {
            if (!CheckAuth()) return Redirect("/admin/");

            return View();
        }
        [Route("Users")]
        public IActionResult Users()
        {
            if (!CheckAuth()) return Redirect("/admin/");

            return View();
        }
        [Route("Mail")]
        public IActionResult Mail()
        {
            if (!CheckAuth()) return Redirect("/admin/");

            return View();
        }
        [Route("Database")]
        public IActionResult Database()
        {
            if (!CheckAuth()) return Redirect("/admin/");

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
