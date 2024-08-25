using EpinelPS.LobbyServer;
using Microsoft.AspNetCore.Mvc;

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
            await LobbyHandler.DispatchSingle(HttpContext);
        }
    }
}
