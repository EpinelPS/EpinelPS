using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/subquestfin/enter")]
    public class EnterFinishSubquest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterSubQuestFinMessengerDialog req = await ReadData<ReqEnterSubQuestFinMessengerDialog>();
            User user = GetUser();

            ResEnterSubQuestFinMessengerDialog response = new();

            KeyValuePair<int, SubquestRecord> opener = GameData.Instance.Subquests.Where(x => x.Key == req.SubQuestId).First();
            KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.Where(x => x.Value.conversation_id == opener.Value.end_messenger_conversation_id && x.Value.is_opener).First();
            
            response.Message = user.CreateMessage(conversation.Value, 1);
            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}
