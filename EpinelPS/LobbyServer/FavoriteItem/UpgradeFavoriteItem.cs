using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/exchange")]
    public class UpgradeFavoriteItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqIncreaseExpFavoriteItem req = await ReadData<ReqIncreaseExpFavoriteItem>();
            User user = GetUser();

            ResEquipFavoriteItem response = new();

            NetUserFavoriteItemData? rFavoriteItem = user.FavoriteItems.FirstOrDefault(f => f.FavoriteItemId == req.FavoriteItemId);
            if (rFavoriteItem == null)
            {
                throw new BadHttpRequestException("Favorite item not found", 400);
            }    

            int srItemTid = rFavoriteItem.Tid + 1;


            NetUserFavoriteItemData? srInventoryItem = user.FavoriteItems.FirstOrDefault(f => f.Tid == srItemTid && f.Csn == 0);
            if (srInventoryItem == null)
            {
                throw new BadHttpRequestException($"No SR-grade favorite item (TID: {srItemTid}) available in inventory for exchange", 400);
            }

            (int NewLevel, int RemainingExp, double ConversionRate) expConversion = CalculateExpConversion(rFavoriteItem.Lv, rFavoriteItem.Exp);

            int exchangeCost = 100000; // 100k gold for exchange
            long goldBefore = user.GetCurrencyVal(CurrencyType.Gold);
            if (goldBefore < exchangeCost)
            {
                throw new BadHttpRequestException($"Insufficient gold for exchange. Required: {exchangeCost}, Available: {goldBefore}", 400);
            }

            long equippedCharacterCsn = rFavoriteItem.Csn;
            
            NetUserFavoriteItemData newSrFavoriteItem = new NetUserFavoriteItemData
            {
                FavoriteItemId = user.GenerateUniqueItemId(),
                Tid = srItemTid,
                Csn = equippedCharacterCsn, // Maintain equipment status
                Lv = expConversion.NewLevel,
                Exp = expConversion.RemainingExp
            };

         
            int itemCountBefore = user.FavoriteItems.Count;
            user.FavoriteItems.Remove(rFavoriteItem);
            user.FavoriteItems.Remove(srInventoryItem);
            int itemCountAfter = user.FavoriteItems.Count;
            user.FavoriteItems.Add(newSrFavoriteItem);
            int finalItemCount = user.FavoriteItems.Count;
         
            user.AddCurrency(CurrencyType.Gold, -exchangeCost);
            long goldAfter = user.GetCurrencyVal(CurrencyType.Gold);


            foreach (NetUserFavoriteItemData item in user.FavoriteItems)
            {
                response.FavoriteItems.Add(item);
            }

            JsonDb.Save();


            await WriteDataAsync(response);
        }

        private static (int NewLevel, int RemainingExp, double ConversionRate) CalculateExpConversion(int rLevel, int rExp)
    {
         int totalRExp = (rLevel * 1000) + rExp;
        
        int totalSRExp = totalRExp;
        
        int srLevel = totalSRExp / 3000; // Each SR level needs 3000 exp
        int remainingExp = totalSRExp % 3000;
        
        if (srLevel > 15)
        {
            srLevel = 15;
            remainingExp = 0;
        }
        
        double conversionRate = totalRExp > 0 ? (double)totalSRExp / totalRExp : 1.0;
        
        return (srLevel, remainingExp, conversionRate);
        }

    }
}
