using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Shop.PackageShop;

[GameRequest("/packageshop/getpopuppackagestate")]
public class GetPackagePopupState : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetPopupPackageState req = await ReadData<ReqGetPopupPackageState>();

        ResGetPopupPackageState response = new();

        // disable ads
        foreach (KeyValuePair<int, PopupPackageListRecord> item in GameData.Instance.PopupPackages)
            response.AppearedList.Add(item.Key);

        await WriteDataAsync(response);
    }
}
