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

        NetMessage? existingMessage = user.MessengerData
            .Where(message => message.ConversationId == opener.Value.EndMessengerConversationId)
            .OrderByDescending(message => message.Seq)
            .FirstOrDefault();

        if (existingMessage != null)
        {
            response.Message = existingMessage;
        }
        else
        {
            int state = conversation.Value.MessageType == MessengerMessageType.Reward ? 1 : 0;
            response.Message = user.CreateMessage(conversation.Value, state);
            JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
}
