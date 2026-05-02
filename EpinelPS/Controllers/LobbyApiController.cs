using EpinelPS.LobbyServer;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EpinelPS.Controllers;

[Route("v1")]
[ApiController]
public class LobbyApiController : ControllerBase
{
    [HttpPost]
    [Route("{**all}", Order = int.MaxValue)]
    [Consumes("application/octet-stream+protobuf")]
    public async Task CatchAll(string all)
    {
        Stopwatch st = Stopwatch.StartNew();
        await LobbyHandler.DispatchSingle(HttpContext);
        st.Stop();

        Logging.WriteLine($"POST {HttpContext.Request.Path.Value}: {HttpContext.Response.StatusCode}", LogType.Info);
    }
}
