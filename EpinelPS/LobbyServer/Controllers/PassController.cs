using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Services;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for unlocking
/// </summary>
[ApiController]
public class PassController(IUserService db) : Controller
{
    [Route("/v1/pass/getactive")]
    [HttpPost]
    public ActionResult<ResGetActivePassData> GetActivePass([FromBodyProtobuf] ReqGetActivePassData req)
    {
        GameUser? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetActivePassData response = new();
        // TODO
        return response;
    }
}
