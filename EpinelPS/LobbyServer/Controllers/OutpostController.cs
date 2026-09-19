using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for outpost
/// </summary>
[ApiController]
public class OutpostController(IUserService UserService, GameContext db) : Controller
{
    [Route("/v1/outpost/recycleroom/get")]
    [HttpPost]
    public ActionResult<ResGetRecycleRoomData> GetMessenger([FromBodyProtobuf] ReqGetRecycleRoomData req)
    {
        GameUser? user = UserService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetRecycleRoomData();
    }
}
