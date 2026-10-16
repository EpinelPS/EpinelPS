using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.EventQuest;

[GameRequest("/eventquest/reward")]
[GameRequest("/eventquest/obtain")]
[GameRequest("/eventquest/reward/obtain")]
[GameRequest("/eventquest/obtainreward")]
[GameRequest("/eventquest/obtain-reward")]
[GameRequest("/event-quest/obtainreward")]
[GameRequest("/event-quest/reward/obtain")]
[GameRequest("/event-quest/reward")]
[GameRequest("/event-quest/obtain")]
public class ObtainEventQuestReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainEventQuestReward req = await ReadData<ReqObtainEventQuestReward>();
        User user = GetUser();
        ResObtainEventQuestReward response = new()
        {
            Reward = new NetRewardData()
        };

        user.ReceivedArchiveEventQuestRewardIds ??= [];

        List<int> rewardIdsToGive = [];
        bool changed = false;
        foreach (int questTid in req.EventQuestTidList)
        {
            if (!user.ReceivedArchiveEventQuestRewardIds.Contains(questTid))
            {
                user.ReceivedArchiveEventQuestRewardIds.Add(questTid);
                changed = true;

                if (GameData.Instance.EventQuestTable.TryGetValue(questTid, out var questRec) && questRec.RewardId > 0)
                {
                    rewardIdsToGive.Add(questRec.RewardId);
                }
            }
        }

        if (rewardIdsToGive.Count > 0)
        {
            response.Reward = RewardUtils.RegisterRewardsForUserDou(user, rewardIdsToGive);
            changed = true;
        }

        if (changed)
        {
            JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
}
