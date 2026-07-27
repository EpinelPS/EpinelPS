using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/obtainachievementmissionreward")]
public class ObtainAchievementMissionReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainRebuildEdenAchievementMissionReward req = await ReadData<ReqObtainRebuildEdenAchievementMissionReward>();
        User user = GetUser();
        ResObtainRebuildEdenAchievementMissionReward response = new();

        NetRewardData ret = new();
        List<int> rewardlist = [];
        EventRebuildEdenManagerRecord_Raw? manger = GameData.Instance.EventRebuildEdenManagerTable.Values
           .Where(x => x.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();

        if (user.RebuildEdenDatas.TryGetValue(manger.Id, out var rebuildEden))
        {
            foreach (var item in req.MissionIds)
            {
                EventRebuildEdenMissionRecord_Raw? mission = GameData.Instance.EventRebuildEdenMissionTable.Values
                    .Where(x => x.Id == item).FirstOrDefault();
                if (mission != null)
                {
                    rewardlist.Add(mission.RewardId);
                }

                rebuildEden.MissionDatas[item].IsReceived = true;
            }

            ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist);
            response.Reward = ret;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}