using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/subquest/enter")]
public class EnterSubquest : LobbyMessage
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
