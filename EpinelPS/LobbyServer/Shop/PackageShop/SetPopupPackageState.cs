using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.PackageShop
{
    [PacketPath("/packageshop/setpopuppackagestate")]
    public class SetPopupPackageState : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetPopupPackageState req = await ReadData<ReqSetPopupPackageState>();

            ResSetPopupPackageState response = new();
            await WriteDataAsync(response);
        }
    }
}
