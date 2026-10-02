using System.Collections.Concurrent;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop.InApp;

internal static class InAppPurchaseHelper
{
    private static readonly ConcurrentDictionary<ulong, ConcurrentDictionary<int, (int ProductType, int ShopTid, int BuyCount)>> SessionClaimedPackages = new();

    public static bool TrySimulatePurchase(User user, string productId, NetStartPurchaseExtraData? extraData,
        out NetRewardData reward)
    {
        reward = new NetRewardData { PassPoint = new NetPassPointData() };
        if (!GameConfig.Root.EnablePurchaseSimulation)
            return false;

        var midas = FindMidasProduct(productId);
        if (midas == null || !midas.IsActive)
            return false;

        var mailItems = new List<NetMailRewardItem>();
        string mailTitle = "Cash Shop Purchase Delivery";

        bool granted = midas.ProductType switch
        {
            ProductType.CashShop => GrantCashShopPurchase(user, midas.ProductId, mailItems, ref mailTitle),
            ProductType.PackageShop => GrantPackageShopPurchase(user, midas.ProductId, mailItems, ref mailTitle),
            ProductType.CostumeShop => GrantCostumeShopPurchase(user, midas.ProductId, mailItems, ref mailTitle),
            ProductType.PassCostumeShop => GrantPassCostumeShopPurchase(user, midas.ProductId, mailItems, ref mailTitle),
            ProductType.MonthlyAmount => GrantMonthlyAmountPurchase(user, midas.ProductId, mailItems, ref mailTitle),
            ProductType.EventInAppShop => GrantEventInAppShopPurchase(user, midas.ProductId, mailItems, ref mailTitle),
            ProductType.TTSAlbumShop => GrantTTSAlbumShop(user, midas.ProductId, ref reward),
            _ => false,
        };

        if (!granted)
            return false;

        if (mailItems.Count > 0)
        {
            DeliverPurchaseToMail(user, mailTitle, mailItems);
        }

        JsonDb.Save();
        return true;
    }

    public static bool HasPendingReward(ulong userId, string productId)
    {
        return false;
    }

    public static void DiscardPendingReceivableProducts(ulong userId, string productId)
    {
    }

    public static NetRewardData TakePendingReward(ulong userId, string productId)
    {
        return new NetRewardData { IsEmptyReward = true, PassPoint = new NetPassPointData() };
    }

    public static List<NetInAppShopReceivableProductData> TakePendingReceivableProducts(ulong userId)
    {
        return [];
    }

    public static bool HasClaimedFreePackageThisSession(ulong userId, int listTid)
    {
        return SessionClaimedPackages.TryGetValue(userId, out var userBuys) && userBuys.ContainsKey(listTid);
    }

    public static void RecordFreePackageClaimThisSession(ulong userId, int listTid, int productType, int shopTid)
    {
        var userBuys = SessionClaimedPackages.GetOrAdd(userId, _ => new ConcurrentDictionary<int, (int, int, int)>());
        userBuys[listTid] = (productType, shopTid, 1);
    }

    public static IReadOnlyDictionary<int, (int ProductType, int ShopTid, int BuyCount)> GetSessionClaimedPackages(ulong userId)
    {
        if (SessionClaimedPackages.TryGetValue(userId, out var userBuys))
        {
            return userBuys;
        }
        return new Dictionary<int, (int, int, int)>();
    }

    public static MidasProductRecord? FindMidasProduct(string productId)
    {
        if (GameData.Instance.mediasProductTable.TryGetValue(productId, out var exact))
            return exact;

        return GameData.Instance.mediasProductTable.Values.FirstOrDefault(x =>
            string.Equals(x.MidasProductIdProximabeta, productId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.MidasProductIdGamamobi, productId, StringComparison.OrdinalIgnoreCase));
    }

    private static List<NetMailRewardItem> GetPackageGroupMailItems(int packageGroupId)
    {
        var items = new List<NetMailRewardItem>();
        var products = GameData.Instance.PackageGroupTable.Values
            .Where(x => x.PackageGroupId == packageGroupId)
            .ToList();

        long expiry = DateTime.UtcNow.AddDays(30).Ticks;
        foreach (var product in products)
        {
            items.Add(new NetMailRewardItem
            {
                RewardType = (int)product.ProductType,
                RewardId = product.ProductId,
                RewardValue = product.ProductValue,
                ExpiredAt = expiry,
            });
        }

        return items;
    }

    private static bool GrantCashShopPurchase(User user, int cashShopId, List<NetMailRewardItem> mailItems, ref string mailTitle)
    {
        if (!GameData.Instance.CashShopRecords.TryGetValue(cashShopId, out var product) || !product.IsActive)
            return false;

        if (!string.IsNullOrEmpty(product.NameLocalkey))
            mailTitle = product.NameLocalkey;

        long expiry = DateTime.UtcNow.AddDays(30).Ticks;
        switch (product.ProductType)
        {
            case CashShopProductType.Currency:
                mailItems.Add(new NetMailRewardItem
                {
                    RewardType = (int)RewardType.Currency,
                    RewardId = product.ProductId,
                    RewardValue = product.ProductValue,
                    ExpiredAt = expiry,
                });
                return true;
            case CashShopProductType.Item:
                mailItems.Add(new NetMailRewardItem
                {
                    RewardType = (int)RewardType.Item,
                    RewardId = product.ProductId,
                    RewardValue = product.ProductValue,
                    ExpiredAt = expiry,
                });
                return true;
            case CashShopProductType.Package:
                mailItems.AddRange(GetPackageGroupMailItems(product.ProductId));
                return true;
            default:
                return false;
        }
    }

    private static bool GrantPackageShopPurchase(User user, int packageShopId, List<NetMailRewardItem> mailItems, ref string mailTitle)
    {
        if (!GameData.Instance.PackageShopTable.TryGetValue(packageShopId, out var package))
            return false;

        mailTitle = "Package Shop Purchase";
        mailItems.AddRange(GetPackageGroupMailItems(package.PackageGroupId));
        return true;
    }

    private static bool GrantCostumeShopPurchase(User user, int costumeShopId, List<NetMailRewardItem> mailItems, ref string mailTitle)
    {
        if (!GameData.Instance.CostumeShopTable.TryGetValue(costumeShopId, out var costume) || !costume.IsActive)
            return false;

        NetRewardData dummy = new();
        AddCostume(user, costume.CostumeId, ref dummy);

        mailTitle = "Costume Purchase";
        if (costume.PackageGroupId > 0)
        {
            mailItems.AddRange(GetPackageGroupMailItems(costume.PackageGroupId));
        }
        return true;
    }

    private static bool GrantPassCostumeShopPurchase(User user, int passCostumeShopId, List<NetMailRewardItem> mailItems, ref string mailTitle)
    {
        if (!GameData.Instance.PassCostumeShopTable.TryGetValue(passCostumeShopId, out var costume))
            return false;

        NetRewardData dummy = new();
        AddCostume(user, costume.CostumeId, ref dummy);

        mailTitle = "Pass Costume Purchase";
        if (costume.PackageGroupId > 0)
        {
            mailItems.AddRange(GetPackageGroupMailItems(costume.PackageGroupId));
        }
        return true;
    }

    private static bool GrantMonthlyAmountPurchase(User user, int monthlyAmountId, List<NetMailRewardItem> mailItems, ref string mailTitle)
    {
        if (!GameData.Instance.MonthlyAmountTable.TryGetValue(monthlyAmountId, out var monthly))
            return false;

        mailTitle = !string.IsNullOrEmpty(monthly.NameLocalkey) ? monthly.NameLocalkey : "30-Day Supply Purchase";
        if (monthly.BuyPackageGroupId > 0)
        {
            mailItems.AddRange(GetPackageGroupMailItems(monthly.BuyPackageGroupId));
        }

        int days = monthly.Period > 0 ? monthly.Period : 30;
        DateTime now = DateTime.UtcNow;
        // Strictly cap to 30 days and 30 remaining claims max for the month (does not stack to 60+ upon repurchase)
        user.MonthlySubscriptions[monthlyAmountId] = now.AddDays(days);
        user.MonthlySubscriptionRemainingClaims[monthlyAmountId] = days;

        return true;
    }

    private static bool GrantEventInAppShopPurchase(User user, int eventInAppShopProductId, List<NetMailRewardItem> mailItems, ref string mailTitle)
    {
        if (!GameData.Instance.EventInAppShopProductTable.TryGetValue(eventInAppShopProductId, out var product))
            return false;

        mailTitle = "Costume Gacha Package";
        if (product.PackageGroupId > 0)
        {
            mailItems.AddRange(GetPackageGroupMailItems(product.PackageGroupId));
        }

        if (user.EventInAppShopBuyCounts.TryGetValue(eventInAppShopProductId, out var count))
        {
            user.EventInAppShopBuyCounts[eventInAppShopProductId] = count + 1;
        }
        else
        {
            user.EventInAppShopBuyCounts[eventInAppShopProductId] = 1;
        }

        return true;
    }

    private static void DeliverPurchaseToMail(User user, string title, List<NetMailRewardItem> items)
    {
        long msn = User.GenerateMsn();
        while (user.MailDatas.ContainsKey(msn))
        {
            msn = User.GenerateMsn();
        }

        NetUserMailData mail = new()
        {
            Sender = 1, // 1 = System / Shifty portrait
            Msn = msn,
            CreatedAt = DateTime.UtcNow.Ticks,
            HasReward = true,
            Nickname = "Cash Shop", // Displays "From. Cash Shop"
            Title = new() { IsPlain = true, Str = title },
            Text = new() { IsPlain = true, Str = $"Items from your {title} purchase have arrived! Thank you for purchasing from the Cash Shop." },
            State = 1, // 1 = Unclaimed
            Type = 1,
            Period = 30
        };
        mail.Items.AddRange(items);
        user.MailDatas.TryAdd(mail.Msn, mail);

        // Ensure Mailbox badge is present with new Seq so red dot appears upon lobby badge sync
        user.LastBadgeSeq = Math.Max(user.LastBadgeSeq, 2000) + 1;
        user.Badges.RemoveAll(b => b.BadgeContent == BadgeContents.Mailbox || b.BadgeContent == BadgeContents.MailboxMessage);
        user.Badges.Add(new BadgeModel
        {
            BadgeContent = BadgeContents.Mailbox,
            BadgeGuid = Guid.NewGuid().ToString(),
            Location = string.Empty,
            Seq = user.LastBadgeSeq
        });
    }

    public static bool GrantPackageGroup(User user, int packageGroupId, ref NetRewardData reward, bool allowEmpty = false)
    {
        var products = GameData.Instance.PackageGroupTable.Values
            .Where(x => x.PackageGroupId == packageGroupId)
            .ToList();
        if (products.Count == 0)
            return allowEmpty;

        foreach (var product in products)
            RewardUtils.AddSingleObject(user, ref reward, product.ProductId, product.ProductType, product.ProductValue);
        return true;
    }

    private static void AddCostume(User user, int costumeId, ref NetRewardData reward)
    {
        if (!user.CostumeList.Contains(costumeId))
            user.CostumeList.Add(costumeId);
        reward.CharacterCostume.Add(costumeId);
    }

    private static bool GrantTTSAlbumShop(User user, int productId, ref NetRewardData reward)
    {
        var album = GameData.Instance.TTSAlbumShopTable.Values
            .FirstOrDefault(x => x.MidasProductId == productId || x.Id == productId)
            ?? GameData.Instance.TTSAlbumShopTable.Values.FirstOrDefault(x => x.Id == 1003);

        int albumId = album?.Id ?? 1003;

        if (!user.TTSGameData.TryGetValue(1, out var ttsData))
        {
            ttsData = new TtsDatas();
            user.TTSGameData[1] = ttsData;
        }

        if (!ttsData.PurchasedAlbumIds.Contains(albumId))
        {
            ttsData.PurchasedAlbumIds.Add(albumId);
        }

        var allSongs = GameData.Instance.EventTTSSongManagerTable.Values.Select(x => x.Id).ToList();
        foreach (var sId in allSongs)
        {
            if (!ttsData.UnlockSongId.Contains(sId))
            {
                ttsData.UnlockSongId.Add(sId);
            }
        }

        return true;
    }
}
