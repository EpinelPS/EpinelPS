using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/proceed")]
public class ProceedMsg : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // This request handles saving user choices
        ReqProceedMessage req = await ReadData<ReqProceedMessage>();
        ResProceedMessage response = new();
        User user = GetUser();

        KeyValuePair<string, MessengerDialogRecord>? msgToSave = GameData.Instance.Messages.FirstOrDefault(x => x.Key == req.MessageId);
        if (msgToSave == null || msgToSave.Value.Value == null)
        {
            await WriteDataAsync(response);
            return;
        }

        string convId = msgToSave.Value.Value.ConversationId;

        // Check if this conversation belongs to an already-completed subquest's end conversation
        var subQuest = GameData.Instance.Subquests.Values.FirstOrDefault(s =>
            !string.IsNullOrEmpty(s.EndMessengerConversationId) && s.EndMessengerConversationId == convId);

        bool isSubQuestDone = subQuest != null &&
            user.SubQuestData.TryGetValue(subQuest.Id, out bool done) && done;

        int state;
        if (msgToSave.Value.Value.MessageType == MessengerMessageType.Reward || msgToSave.Value.Value.RewardId != 0)
        {
            // Subquest reward messages: if already claimed (isSubQuestDone), maintain State = 2; else State = 1 (ready to claim)
            state = isSubQuestDone ? 2 : 1;
        }
        else
        {
            // Normal dialog messages always use State = 0 so players can view and step through conversations
            state = 0;
        }

        NetMessage? existingMessage = user.MessengerData.FirstOrDefault(x => x.MessageId == req.MessageId);
        if (existingMessage != null)
        {
            if (state == 2 && existingMessage.State != 2)
                existingMessage.State = 2;
            else if (state == 1 && existingMessage.State == 0)
                existingMessage.State = 1;
            response.Message = existingMessage;
        }
        else
        {
            response.Message = user.CreateMessage(convId, req.MessageId, state);
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
