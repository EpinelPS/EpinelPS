using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers.System;

/// <summary>
/// Controller for game startup and asset information retrival
/// </summary>
[ApiController]
public class AnticheatController(IUserService db) : Controller
{
    [Route("/v1/antibot/battlereportdata")]
    [HttpPost]
    public ActionResult<ResBattleReportData> ReportBattleData([FromBodyProtobuf] ReqBattleReportData req)
    {
        GameUser? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        return new ResBattleReportData();
    }

    [Route("/v1/antibot/recvdata")]
    [HttpPost]
    public ActionResult<ResAntibotRecvData> RecieveAntibotData([FromBodyProtobuf] ReqAntibotRecvData req)
    {
        GameUser? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        return new ResAntibotRecvData();
    }
}
