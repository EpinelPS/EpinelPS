using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Outpost.Recycle
{
    [PacketPath("/outpost/RecycleRoom/RunResearch")]
    public class RunResearch : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqRecycleRunResearch>();

            var response = new ResRecycleRunResearch();
            // TODO
            
            await WriteDataAsync(response);
        }
    }
}
