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
        User user = GetUser();

        Logging.WriteLine($"[Shop] /shop/buy called by user {user.Nickname}: ShopCategory={req.ShopCategory}, Order={req.Order}, ProductTid={req.ShopProductTid}, Qty={req.Quantity}", LogType.Debug);

        ResShopBuyProduct response = new()
        {
            Product = new NetShopBuyProductData(),
        };

        var product = NormalShopHelper.GetProductById(req.ShopProductTid);
        if (product == null)
        {
            await WriteDataAsync(response);
            return;
        }

        int quantity = req.Quantity;
        int totalPrice = product.PriceValue * quantity;

        if (product.PriceType == ShopProductType.Currency)
        {
            if (!user.CanSubtractCurrency((CurrencyType)product.PriceId, totalPrice))
            {
                await WriteDataAsync(response);
                return;
            }
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
            {
                await WriteDataAsync(response);
                return;
            }
            user.RemoveItemBySerialNumber(costItem.Isn, totalPrice);
        }

        GrantProduct(user, ref response, product, quantity);

        JsonDb.Save();
        await WriteDataAsync(response);
    }

    private void GrantProduct(User user, ref ResShopBuyProduct response, ShopProductRecord product, int quantity)
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