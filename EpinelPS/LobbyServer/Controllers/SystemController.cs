using EpinelPS.Data;
using EpinelPS.Utils;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

[ApiController]
public class SystemController : Controller
{
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
}
