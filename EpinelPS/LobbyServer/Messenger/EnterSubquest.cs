using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/subquest/enter")]
    public class EnterSubquest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterSubQuestMessengerDialog req = await ReadData<ReqEnterSubQuestMessengerDialog>();
            User user = GetUser();

            ResEnterSubQuestMessengerDialog response = new();

            KeyValuePair<int, SubquestRecord> opener = GameData.Instance.Subquests.Where(x => x.Key == req.SubQuestId).First();
            KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.Where(x => x.Value.conversation_id == opener.Value.conversation_id && x.Value.is_opener).First();
            
            response.Message = user.CreateMessage(conversation.Value);
            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}
