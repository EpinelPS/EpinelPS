using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/random/pick")]
    public class GetRandomPick : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqPickTodayRandomMessage req = await ReadData<ReqPickTodayRandomMessage>();

            // TODO: get proper response
            ResPickTodayRandomMessage response = new();

            await WriteDataAsync(response);
        }
    }
}
