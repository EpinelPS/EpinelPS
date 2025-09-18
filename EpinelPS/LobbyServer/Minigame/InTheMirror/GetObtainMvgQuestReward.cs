using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/obtainquestreward")]
    public class GetObtainMvgQuestReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqObtainArcadeMvgQuestReward>();
            
            var user = GetUser();

            List<NetRewardData> rewards = [];

            foreach (var questId in request.QuestIds)
            {
                var quest = GameData.Instance.EventMvgQuestTable[questId];
                if (quest.id != 0)
                {
                    user.ArcadeInTheMirrorData.Quests.First(q => q.QuestId == quest.id).IsReceived = true;

                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, quest.reward_id));
                }
            }

            await WriteDataAsync(new ResObtainArcadeMvgQuestReward() { Reward = NetUtils.MergeRewards(rewards, user) });

            JsonDb.Save();
        }
    }
}
