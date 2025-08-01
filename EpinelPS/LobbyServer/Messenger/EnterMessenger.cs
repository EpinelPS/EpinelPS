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

            MessengerMsgConditionRecord opener = GameData.Instance.MessageConditions[req.Tid];
            KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.Where(x => x.Value.conversation_id == opener.tid && x.Value.is_opener).First();
            
            response.Message = user.CreateMessage(conversation.Value);

            user.AddTrigger(TriggerType.MessageClear, 1, req.Tid); // TODO check if this is correct
            
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
