using EpinelPS.Database;
using EpinelPS.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EpinelPS.Controllers.AdminPanel;

[Route("admin")]
public class AdminController(ILogger<AdminController> logger) : Controller
{
    private readonly ILogger<AdminController> _logger = logger;

    public static bool CheckAuth(HttpContext context)
    {
        string? token = context.Request.Cookies["token"] ?? context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        // TODO better authentication
        if (JsonDb.Instance.AdminAuthTokens.TryGetValue(token, out ulong userId))
        {
            User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
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

	[HttpPost]
	[Route("Users/ModifyEquipment")]
	// Use 'long' for gearIsn to match the EquipmentAwakeningData class
	public IActionResult ModifyEquipment(ulong userId, long gearIsn, int[] baseIds, int[] phases)
	{
		var user = JsonDb.Instance.Users.FirstOrDefault(u => u.ID == userId);
		if(user == null) return NotFound("User not found");

		// Find the piece by its unique Serial Number (Isn)
		var gear = user.EquipmentAwakenings.FirstOrDefault(e => e.Isn == gearIsn);

		if(gear != null && baseIds.Length >= 3 && phases.Length >= 3)
		{
			// Apply the IDs (Base ID + Phase level) 
			// Logic: 7000800 (ATK) + 15 = 7000815
			gear.Option.Option1Id = baseIds[0] + phases[0];
			gear.Option.Option2Id = baseIds[1] + phases[1];
			gear.Option.Option3Id = baseIds[2] + phases[2];

			JsonDb.Save();

			// Redirect back to your Modify page
			return Redirect($"/admin/Users/Modify/{userId}");
		}

		return BadRequest("Equipment ISN not found or invalid data.");
	}
	[HttpPost]
	[Route("Users/ModifyNikkeGear")]
	public IActionResult ModifyNikkeGear(ulong userId, long[] gearIsns, int[] baseIds, int[] phases)
	{
		var user = JsonDb.Instance.Users.FirstOrDefault(u => u.ID == userId);
		if(user == null) return NotFound();

		// Each gear has 3 slots, so we iterate through gearIsns
		// and offset the baseIds/phases by i * 3
		for(int i = 0;i < gearIsns.Length;i++)
		{
			var gear = user.EquipmentAwakenings.FirstOrDefault(e => e.Isn == gearIsns[i]);
			if(gear != null)
			{
				int offset = i * 3;
				gear.Option.Option1Id = baseIds[offset] + phases[offset];
				gear.Option.Option2Id = baseIds[offset + 1] + phases[offset + 1];
				gear.Option.Option3Id = baseIds[offset + 2] + phases[offset + 2];
			}
		}

		JsonDb.Save();
		return Redirect($"/admin/Users/Modify/{userId}");
	}


	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
