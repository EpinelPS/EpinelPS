namespace EpinelPS.LobbyServer.Shop.PackageShop;

[GameRequest("/packageshop/setpopuppackagestate")]
public class SetPopupPackageState : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetPopupPackageState req = await ReadData<ReqSetPopupPackageState>();

        ResSetPopupPackageState response = new();
        await WriteDataAsync(response);
    }
}
