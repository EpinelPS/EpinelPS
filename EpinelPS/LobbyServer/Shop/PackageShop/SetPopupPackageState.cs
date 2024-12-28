using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.PackageShop
{
    [PacketPath("/packageshop/setpopuppackagestate")]
    public class SetPopupPackageState : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetPopupPackageState>();

            var response = new ResSetPopupPackageState();
            await WriteDataAsync(response);
        }
    }
}
