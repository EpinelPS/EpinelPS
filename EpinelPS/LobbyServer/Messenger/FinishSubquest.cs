using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/finsubquest")]
public class FinishSubquest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinSubQuest req = await ReadData<ReqFinSubQuest>();
        User user = GetUser();

        ResFinSubQuest response = new();

        var subQuestEntry = GameData.Instance.Subquests.FirstOrDefault(x => x.Key == req.SubQuestId);
        var conversationEntry = GameData.Instance.Messages.FirstOrDefault(x => x.Value.Id == req.MessageId);

        int rewardId = conversationEntry.Value?.RewardId ?? 0;
        if (rewardId == 0 && subQuestEntry.Value != null)
        {
            // Fallback: look for the reward dialog in the subquest end conversation
            var rewardDialog = GameData.Instance.Messages.Values.FirstOrDefault(m =>
                m.ConversationId == subQuestEntry.Value.EndMessengerConversationId && m.RewardId != 0);
            if (rewardDialog != null)
                rewardId = rewardDialog.RewardId;
        }

        user.SetSubQuest(req.SubQuestId, true);

        NetMessage? conversationRecordUser = user.MessengerData.FirstOrDefault(x => x.MessageId == req.MessageId)
            ?? (subQuestEntry.Value != null ? user.MessengerData.FirstOrDefault(x => x.ConversationId == subQuestEntry.Value.EndMessengerConversationId && x.State != 0) : null);

        if (conversationRecordUser != null)
        {
            if (conversationRecordUser.State == 2)
            {
                // already claimed, don't grant the reward again
                await WriteDataAsync(response);
                return;
            }
            conversationRecordUser.State = 2; // mark as claimed
        }

        if (rewardId != 0)
        {
            RewardRecord? rewardRecord = GameData.Instance.GetRewardTableEntry(rewardId);
            if (rewardRecord != null)
            {
                response.Reward = RewardUtils.RegisterRewardsForUser(user, rewardRecord);
            }
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
