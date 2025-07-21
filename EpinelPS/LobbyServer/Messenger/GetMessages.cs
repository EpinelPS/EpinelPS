using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/get")]
    public class GetMessages : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetMessages req = await ReadData<ReqGetMessages>();
            User user = GetUser();

            ResGetMessages response = new();

            IEnumerable<NetMessage> newMessages = user.MessengerData.Where(x => x.Seq >= req.Seq);

            foreach (NetMessage? item in newMessages)
            {
                response.Messages.Add(item);
            }

            await WriteDataAsync(response);
        }
    }
}
