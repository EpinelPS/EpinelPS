using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.PackageShop
{
    [PacketPath("/packageshop/getpopuppackagestate")]
    public class GetPackagePopupState : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetPopupPackageState req = await ReadData<ReqGetPopupPackageState>();

            ResGetPopupPackageState response = new();

            // disable ads
            foreach (KeyValuePair<int, ProductOfferRecord> item in GameData.Instance.PopupPackages)
                response.AppearedList.Add(item.Key);

            await WriteDataAsync(response);
        }
    }
}
