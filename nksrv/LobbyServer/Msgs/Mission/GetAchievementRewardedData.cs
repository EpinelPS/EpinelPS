using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Mission
{
    [PacketPath("/mission/getrewarded/achievement")]
    public class GetAchievementRewardedData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ResGetAchievementRewardedData>();

            var response = new ReqGetAchievementRewardedData();
            // TODO
            WriteData(response);
        }
    }
}
