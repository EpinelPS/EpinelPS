using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Badge
{
    [PacketPath("/badge/delete")]
    public class DeleteBadge : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqDeleteBadge>();

            var response = new ResDeleteBadge();

            WriteData(response);
        }
    }
}
