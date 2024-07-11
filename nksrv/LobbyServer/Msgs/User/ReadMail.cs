using nksrv.Utils;
using Swan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/mail/read")]
    public class ReadMail : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqReadMail>();

            var r = new ResReadMail();
            //TODO
            await WriteDataAsync(r);
        }
    }
}
