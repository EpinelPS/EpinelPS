using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Shop
{
    [PacketPath("/shop/get")]
    public class GetShop : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqGetShop>();

            var response = new ResGetShop();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
