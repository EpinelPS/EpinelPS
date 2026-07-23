using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/acquireachievementmissionreward")]
public class AcquireAchievementMissionReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAcquireArcadeBubbleMarchAchievementMissionReward req = await ReadData<ReqAcquireArcadeBubbleMarchAchievementMissionReward>();
        User user = GetUser();
        ResAcquireArcadeBubbleMarchAchievementMissionReward response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
                     .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();
        NetRewardData ret = new();
        List<int> rewardlist = [];

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {
            foreach (var item in req.MissionIdList)
            {
                var mission = GameData.Instance.EventBubbleMarchMissionTable.Values
                    .Where(x => x.Id == item).FirstOrDefault();

                rewardlist.Add(mission.RewardId);
                marchData.AchievementMissionDataList[item].IsReceived = true;
            }

            ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist);
            
            response.Reward = ret;
            response.UpdatedAchievementMissions.AddRange(MiniGameHelper
               .ToProtoDict<int, NetMiniGameBubbleMarchMissionData, MiniGameBubbleMarchMissionData>(marchData.AchievementMissionDataList).Values);
        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}