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

        if (!MessengerAccessValidator.CanEnter(user, opener, conversation.Value))
        {
            Logging.WriteLine($"[Messenger] Enter denied: user={user.ID}, Tid={opener.Tid}, RoomId={conversation.Value.RoomId}", LogType.Warning);
            throw new BadHttpRequestException($"Messenger conversation {opener.Tid} is not available for this user", 403);
        }

        NetMessage? existingMessage = user.MessengerData
            .Where(message => message.ConversationId == opener.Tid)
            .OrderByDescending(message => message.Seq)
            .FirstOrDefault();

        if (existingMessage != null)
        {
            response.Message = existingMessage;
        }
        else
        {
            response.Message = user.CreateMessage(conversation.Value);
            user.AddTrigger(Trigger.MessageClear, 1, req.Tid);
            JsonDb.Save();
        }

        // Entering an already-created opener is still a real conversation
        // entry. Record MessageClear exactly once for this condition.
        using (GameContext triggerContext = GameContext.CreateNew())
        {
            if (!triggerContext.Triggers.Any(trigger => trigger.UserId == user.ID &&
                trigger.Type == Trigger.MessageClear && trigger.ConditionId == req.Tid))
            {
                user.AddTrigger(Trigger.MessageClear, 1, req.Tid);
                JsonDb.Save();
            }
        }

        Logging.WriteLine($"[Messenger] Enter: user={user.ID}, Tid={opener.Tid}, RoomId={conversation.Value.RoomId}", LogType.Info);

        await WriteDataAsync(response);
    }
}
