using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for gacha system
/// </summary>
[ApiController]
public class GachaController(IUserService UserService, GameContext db) : Controller
{
    [Route("/v1/Gacha/Get")]
    [HttpPost]
    public ActionResult<ResGetGachaData> GetMessenger([FromBodyProtobuf] ReqGetGachaData req)
    {
        GameUser? user = UserService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetGachaData();
    }
}
