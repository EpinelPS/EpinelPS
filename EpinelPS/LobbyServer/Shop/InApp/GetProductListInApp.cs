using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getdata")]
public class GetProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetInAppShopData>();

        ResGetInAppShopData response = new();
        var now = DateTime.UtcNow;
        // CostumeShop's manager (currently 40005) is marked IsHideIfNotValid
        // in the static data. That flag means "hide when there are no valid
        // products", not "never advertise this manager". The client needs the
        // manager id to load CostumeShopTable rows, so keep it when at least
        // one active costume belongs to its PackageShopId/ShopGroupId.
        var activeCostumeShopGroups = GameData.Instance.CostumeShopTable.Values
            .Where(x => x.IsActive)
            .Where(x => x.StartDate == default || x.EndDate == default ||
                        (x.StartDate <= now && now <= x.EndDate))
            .Select(x => x.ShopGroupId)
            .ToHashSet();

        var activeManagers = GameData.Instance.InAppShopManagerTable.Values
            .Where(x => !x.IsHideIfNotValid ||
                        (x.ShopCategory == InAppShopCategory.CostumeShop &&
                         activeCostumeShopGroups.Contains(x.PackageShopId)))
            .Where(x => x.StartDate == default || x.EndDate == default ||
                        (x.StartDate <= now && now <= x.EndDate))
            .OrderBy(x => x.Id)
            .ToList();

        foreach (var manager in activeManagers)
        {
            response.InAppShopDataList.Add(new NetInAppShopData
            {
                Id = manager.Id,
                StartDate = manager.StartDate == default ? now.Date.Ticks : manager.StartDate.Ticks,
                EndDate = manager.EndDate == default ? now.Date.AddDays(2).Ticks : manager.EndDate.Ticks,
            });
        }

        if (response.InAppShopDataList.Count == 0)
        {
            response.InAppShopDataList.Add(new NetInAppShopData
            {
                Id = 10001,
                StartDate = now.Date.Ticks,
                EndDate = now.Date.AddDays(2).Ticks,
            });
        }

        await WriteDataAsync(response);
    }
}
