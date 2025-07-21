using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/stage/clear")]
    public class ClearSideStoryStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearSideStoryStage req = await ReadData<ReqClearSideStoryStage>();
            User user = GetUser();

            ResClearSideStoryStage response = new();

            if (!user.CompletedSideStoryStages.Contains(req.SideStoryStageId))
            {
                user.CompletedSideStoryStages.Add(req.SideStoryStageId);

                if (GameData.Instance.SidestoryRewardTable.TryGetValue(req.SideStoryStageId, out SideStoryStageRecord? value))
                {
                    RewardTableRecord? rewardData = GameData.Instance.GetRewardTableEntry(value.first_clear_reward);

                    if (rewardData != null)
                        response.Reward = RewardUtils.RegisterRewardsForUser(user, rewardData);
                    else
                        throw new Exception("failed to find reward");
                }

                JsonDb.Save();
            }


            await WriteDataAsync(response);
        }
    }
}
