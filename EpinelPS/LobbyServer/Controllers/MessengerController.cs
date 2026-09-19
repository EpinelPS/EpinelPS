using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for lobby
/// </summary>
[ApiController]
public class MessengerController(IUserService UserService, GameContext db) : Controller
{
    [Route("/v1/messenger/get")]
    [HttpPost]
    public ActionResult<ResGetMessages> GetMessenger([FromBodyProtobuf] ReqGetMessages req)
    {
        GameUser? user = UserService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetMessages();
    }

    [Route("/v1/messenger/random/pick")]
    [HttpPost]
    public ActionResult<ResPickTodayRandomMessage> GetPicked([FromBodyProtobuf] ReqPickTodayRandomMessage req)
    {
        GameUser? user = UserService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResPickTodayRandomMessage();
    }

    [Route("/v1/messenger/picked/get")]
    [HttpPost]
    public ActionResult<ResGetPickedMessageList> GetPicked([FromBodyProtobuf] ReqGetPickedMessageList req)
    {
        GameUser? user = UserService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetPickedMessageList();
    }

    [Route("/v1/messenger/daily/pick")]
    [HttpPost]
    public ActionResult<ResPickTodayDailyMessage> GetPicked([FromBodyProtobuf] ReqPickTodayDailyMessage req)
    {
        GameUser? user = UserService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResPickTodayDailyMessage();
    }
}
