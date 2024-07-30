using EpinelPS.Net;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Sidestory
{
    [PacketPath("/sidestory/cut/clearscenario")]
    public class ClearSideStoryCut : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearSideStoryCutForScenario>();
            var user = GetUser();

            var response = new ResClearSideStoryCutForScenario();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
