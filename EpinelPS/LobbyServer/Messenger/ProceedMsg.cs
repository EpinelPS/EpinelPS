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

        KeyValuePair<string, MessengerDialogRecord>? msgToSave = GameData.Instance.Messages.FirstOrDefault(x => x.Key == req.MessageId);
        if (msgToSave == null || msgToSave.Value.Value == null)
        {
            await WriteDataAsync(response);
            return;
        }

        int state = (msgToSave.Value.Value.MessageType == MessengerMessageType.Reward || msgToSave.Value.Value.RewardId != 0) ? 1 : 0;

        NetMessage? existingMessage = user.MessengerData.FirstOrDefault(x => x.MessageId == req.MessageId);
        if (existingMessage != null)
        {
            if (state == 1 && existingMessage.State == 0)
                existingMessage.State = 1;
            response.Message = existingMessage;
        }
        else
        {
            response.Message = user.CreateMessage(msgToSave.Value.Value.ConversationId, req.MessageId, state);
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
