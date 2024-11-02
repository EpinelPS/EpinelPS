using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Shop
{
    [PacketPath("/productoffer/setseen")]
    public class SeenProductOffer : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqSetSetSeenProductOffer>();

            // TODO: Figure out a way to disable ads

            var response = new ResSetSetSeenProductOffer();

            await WriteDataAsync(response);
        }
    }
}
