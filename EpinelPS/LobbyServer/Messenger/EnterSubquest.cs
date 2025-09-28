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

            KeyValuePair<int, SubQuestRecord> opener = GameData.Instance.Subquests.Where(x => x.Key == req.SubQuestId).First();
            KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.Where(x => x.Value.ConversationId == opener.Value.ConversationId && x.Value.IsOpener).First();
            
            response.Message = user.CreateMessage(conversation.Value);
            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}
