using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/lobby/usertitle/get")]
    public class GetUserTitle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetUserTitleList>();

            var r = new ResGetUserTitleList();

          await  WriteDataAsync(r);
        }
    }
}
