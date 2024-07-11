using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/daily")]
    public class GetDailyRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetDailyRewardedData>();

            // TODO: implement
            var response = new ResGetDailyRewardedData();

          await  WriteDataAsync(response);
        }
    }
}
