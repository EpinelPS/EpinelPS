using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Services;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for stage completion
/// </summary>
[ApiController]
public class SubQuestController(IUserService UserController, GameContext db) : Controller
{

    [Route("/v1/subquest/list")]
    [HttpPost]
    public ActionResult<ResGetSubQuestList> List([FromBodyProtobuf] ReqGetSubQuestList req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetSubQuestList response = new();

        // TODO

        return response;
    }
}
