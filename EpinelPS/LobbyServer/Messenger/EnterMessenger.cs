using EpinelPS.Utils;
using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/enter")]
    public class EnterMessenger : LobbyMsgHandler
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

            user.AddTrigger(Trigger.MessageClear, 1, req.Tid); // TODO check if this is correct

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}