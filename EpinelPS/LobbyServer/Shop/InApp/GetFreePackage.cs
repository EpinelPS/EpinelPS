using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getfreepackage")]
public class GetFreePackage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetFreePackage req = await ReadData<ReqGetFreePackage>();

        NetRewardData reward = PackageRewardBuilder.BuildPackageReward(
            User,
            req.PackageShopTid,
            req.PackageListTid,
            "InAppShop/getfreepackage");

        JsonDb.Save();

        ResGetFreePackage response = new()
        {
            Reward = reward
        };

        await WriteDataAsync(response);
    }
}
