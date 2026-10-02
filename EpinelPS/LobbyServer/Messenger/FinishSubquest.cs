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
            conversationRecordUser.State = 2; // mark reward as claimed
        }

        // Ensure reward messages in this end conversation are marked as claimed (State = 2)
        // Normal text messages remain State = 0 so the player can view and step through dialog
        if (subQuestEntry.Value != null && !string.IsNullOrEmpty(subQuestEntry.Value.EndMessengerConversationId))
        {
            foreach (var msg in user.MessengerData.Where(x => x.ConversationId == subQuestEntry.Value.EndMessengerConversationId))
            {
                if (GameData.Instance.Messages.TryGetValue(msg.MessageId, out var rec) &&
                    (rec.MessageType == MessengerMessageType.Reward || rec.RewardId != 0))
                {
                    msg.State = 2;
                }
            }
        }

        if (subQuestEntry.Value != null)
        {
            if (subQuestEntry.Value.ClearTrigger != Trigger.None)
            {
                user.AddTrigger(subQuestEntry.Value.ClearTrigger, subQuestEntry.Value.ClearConditionValue, subQuestEntry.Value.ClearConditionId);
            }
            user.AddTrigger(Trigger.SubQuestClear, 1, req.SubQuestId);

            // Mark associated substages as completed in user.FieldInfoNew across all trigger types
            int condId = subQuestEntry.Value.ClearConditionId;
            if (condId > 0)
            {
                foreach (CampaignStageRecord stage in GameData.Instance.StageDataRecords.Values)
                {
                    if ((stage.GroupId != 0 && stage.GroupId == condId) || stage.Id == condId)
                    {
                        string stageMapId = GameData.Instance.GetMapIdFromChapter(stage.ChapterId, stage.ChapterMod);
                        if (!user.FieldInfoNew.ContainsKey(stageMapId))
                            user.FieldInfoNew.Add(stageMapId, new FieldInfoNew());
                        if (!user.FieldInfoNew[stageMapId].CompletedStages.Contains(stage.Id))
                            user.FieldInfoNew[stageMapId].CompletedStages.Add(stage.Id);
                    }
                }
            }
        }

        if (rewardId != 0)
        {
            RewardRecord? rewardRecord = GameData.Instance.GetRewardTableEntry(rewardId);
            if (rewardRecord != null)
            {
                response.Reward = RewardUtils.RegisterRewardsForUser(user, rewardRecord);
            }
        }

        // Reconcile eligible subquests (e.g. next subquest in chain)
        MessengerMessageCreator.CreateEligibleSubquestOpeners(user);

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
