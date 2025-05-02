using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/subquestfin/enter")]
    public class EnterFinishSubquest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterSubQuestFinMessengerDialog>();
            var user = GetUser();

            var response = new ResEnterSubQuestFinMessengerDialog();

            var opener = GameData.Instance.Subquests.Where(x => x.Key == req.SubQuestId).First();
            var conversation = GameData.Instance.Messages.Where(x => x.Value.conversation_id == opener.Value.end_messenger_conversation_id && x.Value.is_opener).First();
            
            response.Message = user.CreateMessage(conversation.Value);
            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}
