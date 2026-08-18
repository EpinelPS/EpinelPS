using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/proceed")]
public class ProceedMsg : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // This request handles saving user choices
        ReqProceedMessage req = await ReadData<ReqProceedMessage>();
        ResProceedMessage response = new();
        User user = GetUser();

        KeyValuePair<string, MessengerDialogRecord> msgToSave = GameData.Instance.Messages.Where(x => x.Key == req.MessageId).First();

        // NOTE: reward messages (MessageType Reward) are all subquest completion
        // messages, their reward is granted by /messenger/finsubquest when the
        // user presses the claim button. Do not grant anything here.
        response.Message = user.CreateMessage(msgToSave.Value.ConversationId, req.MessageId);

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
