using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for event fields
/// </summary>
[ApiController]
public class EventFieldController(IUserService UserService, GameContext db) : Controller
{
    [Route("/v1/event/field/password-door/list")]
    [HttpPost]
    public ActionResult<ResListFieldPasswordDoorData> ListPasswordDoors([FromBodyProtobuf] ReqListFieldPasswordDoorData req)
    {
        GameUser? user = UserService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResListFieldPasswordDoorData();
    }
}
