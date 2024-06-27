using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Event
{
    [PacketPath("/event/getjoinedevent")]
    public class EnterLobbyPing : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetJoinedEvent>();

            var response = new ResGetJoinedEvent();

            // TODO

            WriteData(response);
        }
    }
}
