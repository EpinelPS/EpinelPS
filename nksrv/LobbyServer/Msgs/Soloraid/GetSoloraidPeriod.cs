using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Soloraid
{
    [PacketPath("/soloraid/getperiod")]
    public class GetSoloraidPeriod : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSoloRaidPeriod>();

            var response = new ResGetSoloRaidPeriod();
            response.Period = new NetSoloRaidPeriodData
            {
                
            };
            // TODO
            WriteData(response);
        }
    }
}
