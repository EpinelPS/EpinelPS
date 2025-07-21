using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/building")]
    public class BuildBuilding : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqBuilding req = await ReadData<ReqBuilding>();

            ResBuilding response = new()
            {
                StartAt = DateTime.UtcNow.Ticks,
                CompleteAt = DateTime.UtcNow.AddDays(1).Ticks
            };
            // TODO
            await WriteDataAsync(response);
        }
    }
}
