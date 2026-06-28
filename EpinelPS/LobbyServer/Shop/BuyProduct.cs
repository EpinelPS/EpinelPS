using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/buy")]
public class BuyProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopBuyProduct req = await ReadData<ReqShopBuyProduct>();
        ResShopBuyProduct response = new()
        {
            Result = ShopBuyProductResult.Success,
            Product = new NetShopBuyProductData()
        };

        int quantity = Math.Max(1, req.Quantity);

        if (!GameData.Instance.ContentsShopProductTable.TryGetValue(req.ShopProductTid, out ContentsShopProductRecord? product))
        {
            Logging.WriteLine($"Shop/buy missing product request={req}", LogType.Warning);
            response.Result = ShopBuyProductResult.Expired;
            await WriteDataAsync(response);
            return;
        }

        ContentsShopRecord? shop = GameData.Instance.ContentsShopTable.Values
            .FirstOrDefault(candidate => candidate.BundleId == product.BundleId && (int)candidate.ShopCategory == req.ShopCategory);

        if (shop is null)
        {
            Logging.WriteLine(
                $"Shop/buy category mismatch requestCategory={req.ShopCategory} product={product.Id} bundle={product.BundleId}",
                LogType.Warning);
        }

        if (!TryDeductPrice(User, product, quantity, response))
        {
            response.Result = ShopBuyProductResult.Expired;
            await WriteDataAsync(response);
            return;
        }

        NetRewardData reward = new();
        RewardUtils.AddSingleObject(User, ref reward, product.GoodsId, product.GoodsType, product.GoodsValue * quantity);

        response.Product.BuyCount = quantity;
        response.Product.Item.AddRange(reward.Item);
        response.Product.UserItems.AddRange(reward.UserItems);
        response.Product.Currency.AddRange(reward.Currency);
        response.Product.Character.AddRange(reward.Character);
        response.Product.UserCharacters.AddRange(reward.UserCharacters);
        response.Product.AutoCharge.AddRange(reward.AutoChargeList);
        response.Product.LiveWallPapers.AddRange(reward.Livewallpaper);
        response.Product.UserTitleList.AddRange(reward.UserTitleList);

        JsonDb.Save();

        await WriteDataAsync(response);
    }

    private static bool TryDeductPrice(User user, ContentsShopProductRecord product, int quantity, ResShopBuyProduct response)
    {
        int totalPrice = product.PriceValue * quantity;
        if (totalPrice <= 0)
        {
            return true;
        }

        if (product.PriceType == PriceType.Currency)
        {
            CurrencyType currencyType = (CurrencyType)product.PriceId;
            if (!user.SubtractCurrency(currencyType, totalPrice))
            {
                Logging.WriteLine(
                    $"Shop/buy insufficient currency product={product.Id} currency={currencyType} need={totalPrice} have={user.GetCurrencyVal(currencyType)}",
                    LogType.Warning);
                return false;
            }

            response.Currencies.Add(new NetUserCurrencyData
            {
                Type = product.PriceId,
                Value = user.GetCurrencyVal(currencyType)
            });
            return true;
        }

        if (product.PriceType == PriceType.Item)
        {
            DbItemData? item = user.Items.FirstOrDefault(candidate => candidate.ItemType == product.PriceId);
            if (item is null || item.Count < totalPrice)
            {
                Logging.WriteLine(
                    $"Shop/buy insufficient item product={product.Id} item={product.PriceId} need={totalPrice} have={item?.Count ?? 0}",
                    LogType.Warning);
                return false;
            }

            item.Count -= totalPrice;
            response.Item = NetUtils.UserItemDataToNet(item);

            if (item.Count <= 0)
            {
                user.Items.Remove(item);
            }

            return true;
        }

        Logging.WriteLine($"Shop/buy unsupported price type product={product.Id} priceType={product.PriceType}", LogType.Warning);
        return false;
    }
}
