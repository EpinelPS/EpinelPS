using EpinelPS.LobbyServer;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using EpinelPS.Utils;

namespace EpinelPS.Controllers
{
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

            var fg = Console.ForegroundColor;

            if (HttpContext.Response.StatusCode == 200)
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Logging.WriteLine("POST " + HttpContext.Request.Path.Value + " completed in " + st.Elapsed + " with result " + HttpContext.Response.StatusCode, LogType.InfoSuccess);

            Console.ForegroundColor = fg;
        }
    }
}
