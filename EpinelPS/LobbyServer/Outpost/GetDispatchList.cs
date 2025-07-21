using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/dispatch/get")]
    public class GetDispatchList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetDispatchList req = await ReadData<ReqGetDispatchList>();

            ResGetDispatchList response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
