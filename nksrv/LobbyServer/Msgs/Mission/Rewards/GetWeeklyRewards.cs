using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/weekly")]
    public class GetWeeklyRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = ReadData<ReqGetWeeklyRewardedData>();

            // TODO: implement
            var response = new ResGetWeeklyRewardedData();

            WriteData(response);
        }
    }
}
