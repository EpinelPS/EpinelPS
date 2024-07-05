using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Antibot
{
    [PacketPath("/antibot/battlereportdata")]
    public class BattleReportData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqBattleReportData>();
            var response = new ResBattleReportData();

            // this is responsible for server side anticheat

            WriteData(response);
        }
    }
}
