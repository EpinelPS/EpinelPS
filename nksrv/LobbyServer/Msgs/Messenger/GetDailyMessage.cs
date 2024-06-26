using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Messenger
{
    [PacketPath("/messenger/daily/pick")]
    public class GetDailyMessage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqPickTodayDailyMessage>();

            // TODO: save these things
            var response = new ResPickTodayDailyMessage();

            WriteData(response);
        }
    }
}
