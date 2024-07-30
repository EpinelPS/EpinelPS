using EpinelPS.Net;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Sidestory
{
    [PacketPath("/sidestory/stage/enter")]
    public class EnterSidestoryStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterSideStoryStage>();
            var user = GetUser();

            var response = new ResEnterSideStoryStage();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
