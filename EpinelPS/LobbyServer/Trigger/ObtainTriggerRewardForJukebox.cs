using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.TriggerController;

[GameRequest("/Trigger/ObtainTriggerRewardForJukebox")]
public class ObtainTriggerRewardForJukebox : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainTriggerRewardForJukebox req = await ReadData<ReqObtainTriggerRewardForJukebox>();
        User user = GetUser();

        ResObtainTriggerRewardForJukebox response = new();
        List<NetRewardData> rewards = [];

        foreach (var tid in req.TidList.Distinct())
        {
            // give only rewards for milestones that were achieved and not claimed already
            if (user.ClaimedJukeboxRewardTriggers.Contains(tid))
                continue;
            if (!GameData.Instance.TriggerTable.TryGetValue(tid, out var trigger))
                continue;
            if (trigger.Trigger != Trigger.ObtainJukeboxTheme)
                continue;
            if (JukeboxUtils.CountOwnedSongsByTheme(user, trigger.ConditionId) < trigger.ConditionValue)
                continue;

            RewardRecord? reward = GameData.Instance.GetRewardTableEntry(trigger.RewardId);
            if (reward == null)
                continue;

            rewards.Add(RewardUtils.RegisterRewardsForUser(user, reward));
            user.ClaimedJukeboxRewardTriggers.Add(tid);
        }

        response.Reward = NetUtils.MergeRewards(rewards, user);

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
