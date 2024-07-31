using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/building")]
    public class BuildBuilding : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqBuilding>();

            var response = new ResBuilding();
            response.StartAt = DateTime.UtcNow.Ticks;
            response.CompleteAt = DateTime.UtcNow.AddDays(1).Ticks;
            // TODO
            await WriteDataAsync(response);
        }
    }
}
