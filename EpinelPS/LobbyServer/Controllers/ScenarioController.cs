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
/// Controller for user data
/// </summary>
[ApiController]
public class ScenarioController(IUserService userService, GameContext db) : Controller
{
    [Route("/v1/user/scenario/exist")]
    [HttpPost]
    public ActionResult<ResExistScenario> GetUserTitle([FromBodyProtobuf] ReqExistScenario req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResExistScenario();
    }
}
