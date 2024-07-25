using nksrv.Database;
using nksrv.LobbyServer.Msgs.Stage;
using nksrv.Net;
using nksrv.StaticInfo;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Sidestory
{
    [PacketPath("/sidestory/stage/clear")]
    public class ClearSideStoryStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearSideStoryStage>();
            var user = GetUser();

            var response = new ResClearSideStoryStage();

            if (!user.CompletedSideStoryStages.Contains(req.SideStoryStageId))
            {
                user.CompletedSideStoryStages.Add(req.SideStoryStageId);

                if (StaticDataParser.Instance.SidestoryRewardTable.ContainsKey(req.SideStoryStageId))
                {
                    var rewardData = StaticDataParser.Instance.GetRewardTableEntry(StaticDataParser.Instance.SidestoryRewardTable[req.SideStoryStageId]);

                    if (rewardData != null)
                        response.Reward = ClearStage.RegisterRewardsForUser(user, rewardData);
                    else
                        throw new Exception("failed to find reward");
                }

                JsonDb.Save();
            }


            await WriteDataAsync(response);
        }
    }
}
