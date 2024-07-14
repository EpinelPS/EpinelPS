using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Arena
{
    [PacketPath("/arena/special/showreward")]
    public class ShowSpecialArenaReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqShowSpecialArenaReward>();

            var response = new ResShowSpecialArenaReward();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
