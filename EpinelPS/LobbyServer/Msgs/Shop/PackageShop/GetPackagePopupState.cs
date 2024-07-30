using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Shop.PackageShop
{
    [PacketPath("/packageshop/getpopuppackagestate")]
    public class GetPackagePopupState : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetPopupPackageState>();

            var response = new ResGetPopupPackageState();
            await WriteDataAsync(response);
        }
    }
}
