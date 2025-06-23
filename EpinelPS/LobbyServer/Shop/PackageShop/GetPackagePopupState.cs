using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.PackageShop
{
    [PacketPath("/packageshop/getpopuppackagestate")]
    public class GetPackagePopupState : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetPopupPackageState>();

            var response = new ResGetPopupPackageState();

            // disable ads
            foreach (var item in GameData.Instance.PopupPackages)
                response.AppearedList.Add(item.Key);

            await WriteDataAsync(response);
        }
    }
}
