using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop
{
    [PacketPath("/productoffer/setseen")]
    public class SeenProductOffer : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetSetSeenProductOffer x = await ReadData<ReqSetSetSeenProductOffer>();

            // TODO: Figure out a way to disable ads

            ResSetSetSeenProductOffer response = new();

            await WriteDataAsync(response);
        }
    }
}
