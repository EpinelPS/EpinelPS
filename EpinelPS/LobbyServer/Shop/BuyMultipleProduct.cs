using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/shop/multiple-buy")]
public class BuyMultipleProduct : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShopBuyMultipleProduct req = await ReadData<ReqShopBuyMultipleProduct>();
        User user = GetUser();

        string details = string.Join(", ", req.Products.Select(p => $"Tid={p.ShopProductTid},Order={p.Order},Qty={p.Quantity}"));
        Logging.WriteLine($"[Shop] /shop/multiple-buy called by user {user.Nickname}: ShopCategory={req.ShopCategory}, Products=[{details}]", LogType.Debug);

        ResShopBuyMultipleProduct response = new()
        {
            Product = new NetShopBuyMultipleProductData(),
        };

        foreach (var buyReq in req.Products)
        {
            var product = NormalShopHelper.GetProductById(buyReq.ShopProductTid);
            if (product == null) continue;

            int quantity = buyReq.Quantity;
            int totalPrice = product.PriceValue * quantity;

            if (product.PriceType == ShopProductType.Currency)
            {
                if (!user.CanSubtractCurrency((CurrencyType)product.PriceId, totalPrice))
                    continue;
                user.SubtractCurrency(product.PriceId, totalPrice);
                response.Currencies.Add(new NetUserCurrencyData
                {
                    Type = (int)product.PriceId,
                    Value = user.GetCurrencyVal((CurrencyType)product.PriceId),
                });
            }
            else if (product.PriceType == ShopProductType.Item)
            {
                var costItem = user.Items.FirstOrDefault(i => i.ItemType == (int)product.PriceId);
                if (costItem == null || costItem.Count < totalPrice)
                    continue;
                user.RemoveItemBySerialNumber(costItem.Isn, totalPrice);
            }

            GrantProduct(user, ref response, product, quantity, buyReq.Order);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }

    private void GrantProduct(User user, ref ResShopBuyMultipleProduct response, ShopProductRecord product, int quantity, int order)
    {
        int totalValue = product.ProductValue * quantity;

        if (product.ProductType == ShopProductType.Currency)
        {
            user.AddCurrency((CurrencyType)product.ProductId, totalValue);
            response.Product.Currency.Add(new NetCurrencyData
            {
                Type = product.ProductId,
                Value = totalValue,
                FinalValue = user.GetCurrencyVal((CurrencyType)product.ProductId),
            });
        }
        else if (product.ProductType == ShopProductType.Item)
        {
            var existing = user.Items.FirstOrDefault(i => i.ItemType == product.ProductId);
            if (existing != null)
            {
                existing.Count += totalValue;
                response.Product.Item.Add(NetUtils.ItemDataToNet(existing));
                response.Product.UserItems.Add(NetUtils.UserItemDataToNet(existing));
            }
            else
            {
                var newItem = new DbItemData
                {
                    ItemType = product.ProductId,
                    Count = totalValue,
                    Isn = user.GenerateUniqueItemId(),
                };
                user.Items.Add(newItem);
                response.Product.Item.Add(NetUtils.ItemDataToNet(newItem));
                response.Product.UserItems.Add(NetUtils.UserItemDataToNet(newItem));
            }
        }
        else if (product.ProductType == ShopProductType.Character)
        {
            if (GameData.Instance.CharacterTable.TryGetValue(product.ProductId, out var charRecord))
            {
                var userChar = user.GetCharacter(product.ProductId);
                if (userChar == null)
                {
                    userChar = new CharacterModel
                    {
                        Csn = user.GenerateUniqueCharacterId(),
                        Grade = 1,
                        Tid = charRecord.Id,
                    };
                    user.Characters.Add(userChar);
                }
                response.Product.Character.Add(new NetCharacterData
                {
                    Csn = userChar.Csn,
                    Tid = userChar.Tid,
                    PieceCount = totalValue,
                });
            }
        }
    }
}