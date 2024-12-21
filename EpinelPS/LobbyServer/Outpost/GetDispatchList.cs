using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/dispatch/get")]
    public class GetDispatchList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetDispatchList>();

            var response = new ResGetDispatchList();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
