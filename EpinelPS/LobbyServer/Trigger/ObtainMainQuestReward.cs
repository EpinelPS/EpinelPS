using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.TriggerController;

[GameRequest("/trigger/obtainmainquestreward")]
public class ObtainMainQuestReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainMainQuestReward req = await ReadData<ReqObtainMainQuestReward>();
        User user = GetUser();

        ResObtainMainQuestReward response = new();
        List<NetRewardData> rewards = [];

        foreach (KeyValuePair<int, bool> item in user.MainQuestData)
        {
            // give only rewards for things that were completed and not claimed already
            if (!item.Value && req.TidList.Contains(item.Key))
            {
                user.MainQuestData[item.Key] = true;

                MainQuestRecord? questInfo = GameData.Instance.GetMainQuestByTableId(item.Key);
                if (questInfo == null)
                {
                    Logging.Warn($"[ObtainMainQuestReward] Failed to lookup quest Id {item.Key}");
                    continue;
                }

                RewardRecord? reward = GameData.Instance.GetRewardTableEntry(questInfo.RewardId);
                if (reward != null)
                {
                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, reward));
                }
                else
                {
                    Logging.Warn($"[ObtainMainQuestReward] Failed to lookup reward Id {questInfo.RewardId} for quest {item.Key}");
                }
            }
        }

        response.Reward = NetUtils.MergeRewards(rewards, user);

        foreach (NetItemData? item in response.Reward.Item)
        {
            Console.WriteLine($"item: {item.Tid} {item.Isn} {item.Count}");
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
