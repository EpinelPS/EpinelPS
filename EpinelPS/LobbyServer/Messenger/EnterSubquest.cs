using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/subquest/enter")]
public class EnterSubquest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterSubQuestMessengerDialog req = await ReadData<ReqEnterSubQuestMessengerDialog>();
        User user = GetUser();

        ResEnterSubQuestMessengerDialog response = new();
        var opener = GameData.Instance.Subquests.FirstOrDefault(x => x.Key == req.SubQuestId);
        if (opener.Value == null)
        {
            Logging.Warn($"Subquest {req.SubQuestId} not found.");
            return;
        }

        var conversation = GameData.Instance.Messages.FirstOrDefault(x =>
            x.Value.ConversationId == opener.Value.ConversationId &&
            x.Value.IsOpener);

        if (conversation.Value == null)
        {
            Logging.Warn($"Subquest {req.SubQuestId} not found.");
            return;
        }

        response.Message = user.CreateMessage(conversation.Value);
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
