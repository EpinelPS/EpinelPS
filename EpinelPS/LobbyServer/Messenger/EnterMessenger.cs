using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/enter")]
public class EnterMessenger : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterMessengerDialog req = await ReadData<ReqEnterMessengerDialog>();
        User user = GetUser();

        ResEnterMessengerDialog response = new();

        if (!GameData.Instance.MessageConditions.TryGetValue(req.Tid, out MessengerConditionTriggerRecord? opener))
        {
            throw new BadHttpRequestException($"Message condition {req.Tid} not found", 404);
        }

        KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.FirstOrDefault(x =>
            x.Value.ConversationId == opener.Tid && x.Value.IsOpener);

        if (conversation.Value == null)
        {
            conversation = GameData.Instance.Messages.FirstOrDefault(x =>
                x.Value.ConversationId == opener.Tid);

            if (conversation.Value == null)
            {
                throw new BadHttpRequestException($"No conversation found for {opener.Tid}", 404);
            }
        }

        response.Message = user.CreateMessage(conversation.Value);

        Logging.WriteLine($"[Messenger] Enter: user={user.ID}, Tid={opener.Tid}, RoomId={conversation.Value.RoomId}", LogType.Info);
        user.AddTrigger(Trigger.MessageClear, 1, req.Tid);

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
