using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/subquestfin/enter")]
public class EnterFinishSubquest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterSubQuestFinMessengerDialog req = await ReadData<ReqEnterSubQuestFinMessengerDialog>();
        User user = GetUser();

        ResEnterSubQuestFinMessengerDialog response = new();

        var opener = GameData.Instance.Subquests.FirstOrDefault(x => x.Key == req.SubQuestId);
        if (opener.Value == null)
        {
            Logging.Warn($"Subquest {req.SubQuestId} not found.");
            await WriteDataAsync(response);
            return;
        }

        var conversation = GameData.Instance.Messages.FirstOrDefault(x =>
            x.Value.ConversationId == opener.Value.EndMessengerConversationId && x.Value.IsOpener);

        if (conversation.Value == null)
        {
            Logging.Warn($"End conversation for subquest {req.SubQuestId} not found.");
            await WriteDataAsync(response);
            return;
        }

        bool isCompleted = user.SubQuestData.TryGetValue(req.SubQuestId, out bool done) && done;
        if (isCompleted && !string.IsNullOrEmpty(opener.Value.EndMessengerConversationId))
        {
            foreach (var msg in user.MessengerData.Where(message => message.ConversationId == opener.Value.EndMessengerConversationId))
            {
                if (GameData.Instance.Messages.TryGetValue(msg.MessageId, out var rec) &&
                    (rec.MessageType == MessengerMessageType.Reward || rec.RewardId != 0))
                {
                    msg.State = 2;
                }
            }
        }

        NetMessage? existingMessage = user.MessengerData
            .Where(message => message.ConversationId == opener.Value.EndMessengerConversationId)
            .OrderByDescending(message => message.Seq)
            .FirstOrDefault();

        if (existingMessage != null)
        {
            if (isCompleted && GameData.Instance.Messages.TryGetValue(existingMessage.MessageId, out var rec) &&
                (rec.MessageType == MessengerMessageType.Reward || rec.RewardId != 0))
            {
                existingMessage.State = 2;
            }
            response.Message = existingMessage;
        }
        else
        {
            int state = isCompleted ? 2 : (conversation.Value.MessageType == MessengerMessageType.Reward ? 1 : 0);
            response.Message = user.CreateMessage(conversation.Value, state);
            JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
}
