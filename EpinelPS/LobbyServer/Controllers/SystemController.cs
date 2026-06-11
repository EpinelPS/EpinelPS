using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for game startup and asset information retrival
/// </summary>
[ApiController]
public class SystemController : Controller
{
    /// <summary>
    /// Returns latest resource base URL for version number
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Route("/v1/resourcehosts2")]
    [HttpPost]
    public ActionResult<ResGetResourceHosts2> GetResourceHosts([FromBodyProtobuf] ReqGetResourceHosts2 req)
    {
        return new ResGetResourceHosts2()
        {
            BaseUrl = GameConfig.Root.ResourceBaseURL,
            Version = req.Version
        };
    }
    /// <summary>
    /// Retrieves latest static info URL, hash, and keys
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Route("/v1/get-static-data-pack-info-mpk")]
    [HttpPost]
    public ActionResult<ResStaticDataPackInfoMpk> GetStaticData([FromBodyProtobuf] ReqStaticDataPackInfoMpk req)
    {
        StaticData data = GameConfig.Root.StaticDataMpk;

        return new ResStaticDataPackInfoMpk()
        {
            Url = data.Url,
            Version = data.Version,
            Size = GameData.Instance.MpkSize,
            Sha256Sum = ByteString.CopyFrom(GameData.Instance.MpkHash),
            Salt1 = ByteString.CopyFrom(Convert.FromBase64String(data.Salt1)),
            Salt2 = ByteString.CopyFrom(Convert.FromBase64String(data.Salt2))
        };
    }

    /// <summary>
    /// Checks if game version is compatible with the server version
    /// </summary>
    /// <param name="req">Version Request</param>
    /// <returns>Version validity response</returns>
    [Route("/v1/system/checkversion")]
    [HttpPost]
    public ActionResult<ResCheckClientVersion> CheckVersion([FromBodyProtobuf] ReqCheckClientVersion req)
    {
        ResCheckClientVersion response = new()
        {
            Availability = ResCheckClientVersion.Types.Availability.None
        };

        if (req.Version != GameConfig.Root.TargetVersion)
        {
            Logging.Warn($"Game verion mismatch, game version {req.Version} while server is {GameConfig.Root.TargetVersion}");
            response.Availability = ResCheckClientVersion.Types.Availability.Available;
        }

        return response;
    }

    /// <summary>
    /// Gets server UTC time and reset hour. Client checks if the system time differs from server time (authenticated)
    /// </summary>
    /// <param name="req">Version Request</param>
    /// <returns>Version validity response</returns>
    [Route("/v1/now")]
    [HttpPost]
    public ActionResult<ResGetNow> GetTime([FromBodyProtobuf] ReqGetNow req)
    {
        return new ResGetNow()
        {
            Tick = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ResetHour = JsonDb.Instance.ResetHourUtcTime,
            CheatShiftDuration = Duration.FromTimeSpan(TimeSpan.FromSeconds(0))
        };
    }
}
