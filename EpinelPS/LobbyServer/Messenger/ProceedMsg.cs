using EpinelPS.Utils;
using EpinelPS.StaticInfo;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/proceed")]
    public class ProceedMsg : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // This request handles saving user choices
            var req = await ReadData<ReqProceedMessage>();
            ResProceedMessage response = new();
            var user = GetUser();

            var msgToSave = GameData.Instance.Messages.Where(x => x.Key == req.MessageId).First();

            response.Message = user.CreateMessage(msgToSave.Value.conversation_id, req.MessageId);

            if (msgToSave.Value.reward_id != 0)
            {
                Console.WriteLine("TODO reward for messenger. Reward ID: " + msgToSave.Value.reward_id + " Message ID: " + req.MessageId);
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
