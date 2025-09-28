using EpinelPS.Utils;
using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/proceed")]
    public class ProceedMsg : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // This request handles saving user choices
            ReqProceedMessage req = await ReadData<ReqProceedMessage>();
            ResProceedMessage response = new();
            User user = GetUser();

            KeyValuePair<string, MessengerDialogRecord> msgToSave = GameData.Instance.Messages.Where(x => x.Key == req.MessageId).First();

            response.Message = user.CreateMessage(msgToSave.Value.ConversationId, req.MessageId);

            if (msgToSave.Value.RewardId != 0)
            {
                Logging.WriteLine("TODO reward for messenger. Reward ID: " + msgToSave.Value.RewardId + " Message ID: " + req.MessageId, LogType.Warning);
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
