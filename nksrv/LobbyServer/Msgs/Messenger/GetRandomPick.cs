using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Messenger
{
    [PacketPath("/messenger/random/pick")]
    public class GetRandomPick : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqForcePickTodayRandomMessage>();

            // TODO: get proper response
            var response = new ResForcePickTodayRandomMessage();

            WriteData(response);
        }
    }
}
