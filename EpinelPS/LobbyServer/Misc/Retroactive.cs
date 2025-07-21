using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/lobby/retroactive")]
    public class LobbyRetroactive : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqRetroactive req = await ReadData<ReqRetroactive>();
            Database.User user = GetUser();

            ResRetroactive response = new();
            await WriteDataAsync(response);
        }
    }
}
