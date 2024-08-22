using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Misc
{
    [PacketPath("/lobby/retroactive")]
    public class LobbyRetroactive : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqRetroactive>();
            var user = GetUser();

            var response = new ResRetroactive();
            await WriteDataAsync(response);
        }
    }
}
