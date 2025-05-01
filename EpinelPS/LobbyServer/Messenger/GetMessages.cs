using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/get")]
    public class GetMessages : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMessages>();
            var user = GetUser();

            var response = new ResGetMessages();

            var newMessages = user.MessengerData.Where(x => x.Seq >= req.Seq);

            foreach (var item in newMessages)
            {
                response.Messages.Add(item);
            }

            await WriteDataAsync(response);
        }
    }
}
