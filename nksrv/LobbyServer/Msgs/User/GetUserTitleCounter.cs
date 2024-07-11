using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/lobby/usertitlecounter/get")]
    public class GetUserTitleCounter : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetUserTitleCounterList>();

            var r = new ResGetUserTitleCounterList();

            await WriteDataAsync(r);
        }
    }
}
