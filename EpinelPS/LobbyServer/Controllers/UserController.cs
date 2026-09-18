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
public class UserController(IUserService userService, GameContext db) : Controller
{
    [Route("/v1/lobby/usertitle/get")]
    [HttpPost]
    public ActionResult<ResGetUserTitleList> GetUserTitle([FromBodyProtobuf] ReqGetUserTitleList req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetUserTitleList();
    }

    [Route("/v1/lobby/usertitlecounter/get")]
    [HttpPost]
    public ActionResult<ResGetUserTitleCounterList> GetUserTitle([FromBodyProtobuf] ReqGetUserTitleCounterList req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetUserTitleCounterList();
    }

    [Route("/v1/useronlinestatelog")]
    [HttpPost]
    public async Task<ActionResult<ResUserOnlineStateLog>> LastAction([FromBodyProtobuf] ReqUserOnlineStateLog req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        await db.Users.Where(u => u.ID == user.ID).ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastAction, DateTime.UtcNow));
        await db.SaveChangesAsync();
        return new ResUserOnlineStateLog();
    }
}
