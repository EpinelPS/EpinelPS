using EpinelPS.Database;
using EpinelPS.Models.Admin;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;
using Paseto;
using Paseto.Builder;
using System.Diagnostics;

namespace EpinelPS.Controllers.AdminPanel;

[Route("admin")]
public class AdminController(ILogger<AdminController> logger) : Controller
{
    private readonly ILogger<AdminController> _logger = logger;

    public static bool CheckAuth(HttpContext context)
    {
        var db = context.RequestServices.GetRequiredService<GameContext>();
        string? token = context.Request.Cookies["token"] ?? context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        PasetoTokenValidationResult encryptionToken = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                .Decode(token, new PasetoTokenValidationParameters() { ValidateLifetime = true });

        if (encryptionToken.IsValid)
        {
            var id = ((System.Text.Json.JsonElement)encryptionToken.Paseto.Payload["userId"]).GetUInt64();

            if (id == 0) return false;

            return db.SdkUsers.Where(x => x.ID == id && x.IsAdmin).Any();
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

        return View(JsonDb.Instance.ActiveEventBannerIds);
    }

    [Route("Events"), ActionName("Events")]
    [HttpPost]
    public IActionResult EventsSave([FromForm] List<int>? activeBannerIds)
    {
        if (!CheckAuth(HttpContext)) return Redirect("/admin/");

        JsonDb.Instance.ActiveEventBannerIds = activeBannerIds ?? [];
        JsonDb.Save();

        TempData["MessageKey"] = "events.config.saved";
        return View(JsonDb.Instance.ActiveEventBannerIds);
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
    [Route("Search")]
    public IActionResult Search()
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
