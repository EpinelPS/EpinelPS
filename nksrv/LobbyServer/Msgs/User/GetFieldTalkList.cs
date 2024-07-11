using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/user/getfieldtalklist")]
    public class GetFieldTalkList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetFieldTalkList>();
            var user = GetUser();

            var response = new ResGetFieldTalkList();
            // TODO

          await  WriteDataAsync(response);
        }
    }
}
