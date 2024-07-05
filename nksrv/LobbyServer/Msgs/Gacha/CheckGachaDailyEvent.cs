using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Gacha
{
    [PacketPath("/gacha/event/check")]
    public class CheckGachaDailyEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckDailyFreeGacha>();

            var response = new ResCheckDailyFreeGacha();

            // TODO implement
            response.FreeCount = 0;
            response.EventData = new NetEventData() { Id = 1 };

            WriteData(response);
        }
    }
}
