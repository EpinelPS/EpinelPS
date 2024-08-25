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
        [Route("index")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }
        [Route("Events")]
        public IActionResult Events()
        {
            return View();
        }
        [Route("Configuration")]
        public IActionResult Configuration()
        {
            return View();
        }
        [Route("Users")]
        public IActionResult Users()
        {
            return View();
        }
        [Route("Mail")]
        public IActionResult Mail()
        {
            return View();
        }
        [Route("Database")]
        public IActionResult Database()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
