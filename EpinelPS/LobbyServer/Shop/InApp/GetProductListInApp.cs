using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/getdata")]
public class GetProductList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetInAppShopData>();

        ResGetInAppShopData response = new();

        long startDate = DateTime.Now.AddDays(-1).Ticks;
        long endDate = DateTime.Now.AddDays(2).Ticks;

        IReadOnlyList<InAppShopManagerRecord> managers = BuildVisibleShopManagers();

        foreach (InAppShopManagerRecord manager in managers)
        {
            response.InAppShopDataList.Add(new NetInAppShopData()
            {
                Id = manager.Id,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        await WriteDataAsync(response);
    }

    private static IReadOnlyList<InAppShopManagerRecord> BuildVisibleShopManagers()
    {
        (CashshopMainCategoryType Category, int Limit)[] categoryLimits =
        [
            (CashshopMainCategoryType.JewelTab, 4),
            (CashshopMainCategoryType.PopupPackageTab, 3),
            (CashshopMainCategoryType.TimeLimitPackageTab, 8),
            (CashshopMainCategoryType.RenewPackageTab, 4),
            (CashshopMainCategoryType.MonthlyAmountTab, 8),
            (CashshopMainCategoryType.CampaignPackageTab, 4),
            (CashshopMainCategoryType.CostumeTab, 8),
        ];

        List<InAppShopManagerRecord> managers = [];

        foreach ((CashshopMainCategoryType category, int limit) in categoryLimits)
        {
            List<InAppShopManagerRecord> categoryManagers = GameData.Instance.InAppShopManagerTable.Values
                .Where(manager => manager.MainCategoryType == category)
                .Where(PackagePurchaseResolver.HasValidPackageProducts)
                .OrderBy(manager => manager.IsHideIfNotValid)
                .ThenBy(manager => manager.SubCategoryId)
                .ThenBy(manager => manager.Id)
                .Take(limit)
                .ToList();

            managers.AddRange(categoryManagers);
        }

        if (managers.Count == 0 && GameData.Instance.InAppShopManagerTable.TryGetValue(10001, out InAppShopManagerRecord? fallback))
        {
            managers.Add(fallback);
        }

        return managers;
    }
}
