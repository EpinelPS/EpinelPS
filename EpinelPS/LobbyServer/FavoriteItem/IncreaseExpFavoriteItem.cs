using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/increaseexp")]
    public class IncreaseExpFavoriteItem : LobbyMsgHandler
    {
        // Favorite item experience materials mapping
        private static readonly Dictionary<int, int> favoriteExpTable = new()
        {
            { 7150001, 20 },    // 20 exp
            { 7150002, 50 },    // 50 exp
            { 7150003, 100 },   // 100 exp
        };

        protected override async Task HandleAsync()
        {
            ReqIncreaseExpFavoriteItem req = await ReadData<ReqIncreaseExpFavoriteItem>();
            User user = GetUser();

            ResIncreaseExpFavoriteItem response = new();

            NetUserFavoriteItemData? favoriteItem = user.FavoriteItems.FirstOrDefault(f => f.FavoriteItemId == req.FavoriteItemId);
            if (favoriteItem == null)
            {
                throw new BadHttpRequestException($"FavoriteItem with ID {req.FavoriteItemId} not found", 404);
            }

            if (req.ItemData == null)
            {
                throw new BadHttpRequestException($"No material item provIded", 400);
            }

            ItemData? userItem = user.Items.FirstOrDefault(x => x.Isn == req.ItemData.Isn);
            if (userItem == null)
            {
                throw new BadHttpRequestException($"Material item with ISN {req.ItemData.Isn} not found", 404);
            }

            int useCount = req.ItemData.Count * req.LoopCount;
            if (userItem.Count < useCount)
            {
                throw new BadHttpRequestException($"Insufficient material. Required: {useCount}, Available: {userItem.Count}", 400);
            }

            FavoriteItemProbabilityRecord? probabilityData = GetProbabilityData(favoriteItem.Lv, req.ItemData.Tid);
            if (probabilityData == null)
            {
                throw new BadHttpRequestException($"Cannot upgrade at current level with this material", 400);
            }

            int baseExp = probabilityData.Exp * req.LoopCount;
            bool isGreatSuccess = CheckGreatSuccess(probabilityData.GreatSuccessRate);

            int totalExpGained = baseExp;
            int targetLevel = favoriteItem.Lv;

            if (isGreatSuccess)
            {
                targetLevel = probabilityData.GreatSuccessRate;
            }

            int goldCost = baseExp * 10;

            userItem.Count -= useCount;

            if (user.GetCurrencyVal(CurrencyType.Gold) < goldCost)
            {
                throw new BadHttpRequestException($"Insufficient gold. Required: {goldCost}, Available: {user.GetCurrencyVal(CurrencyType.Gold)}", 400);
            }

            int originalLevel = favoriteItem.Lv;
            int originalExp = favoriteItem.Exp;

            if (isGreatSuccess)
            {
                favoriteItem.Lv = targetLevel;
                favoriteItem.Exp = 0; // Reset exp at target level
            }
            else
            {
                favoriteItem.Exp += totalExpGained;
                ProcessLevelUp(favoriteItem);
            }

            user.AddCurrency(CurrencyType.Gold, -goldCost);

            
            response.FavoriteItem = favoriteItem;
            response.Result = isGreatSuccess ? FavoriteItemGreatSuccessResult.GreatSuccess : FavoriteItemGreatSuccessResult.Success;
            response.ItemData = NetUtils.ToNet(userItem);
            response.LoopCount = req.LoopCount;

      
            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private FavoriteItemProbabilityRecord? GetProbabilityData(int currentLevel, int materialId)
        {
            foreach (var record in GameData.Instance.FavoriteItemProbabilityTable.Values)
            {
                if (record.NeedItemId == materialId &&
                    currentLevel >= record.LevelMin &&
                    currentLevel <= record.LevelMax)
                {
                    return record;
                }
            }
            return null;
        }

        private bool CheckGreatSuccess(int successRate)
        {
            Random random = new Random();
            int roll = random.Next(0, 10000);
            return roll < successRate;
        }
                                   
        private void ProcessLevelUp(NetUserFavoriteItemData favoriteItem)
        {
              
            if (!GameData.Instance.FavoriteItemTable.TryGetValue(favoriteItem.Tid, out FavoriteItemRecord? favoriteRecord))
            {
                var sampleTIds = GameData.Instance.FavoriteItemTable.Keys.Take(5).ToList();
                return;
            }

            
            var itemRarity = favoriteRecord.FavoriteRare;
            if (favoriteRecord.FavoriteRare == FavoriteItemRare.None)
            {
                // TODO: dont hardcode table ids
                if (favoriteItem.Tid >= 100102 && favoriteItem.Tid <= 100602 && favoriteItem.Tid % 100 == 2)
                {
                    itemRarity = FavoriteItemRare.SR;
                }
                else if (favoriteItem.Tid >= 100101 && favoriteItem.Tid <= 100601 && favoriteItem.Tid % 100 == 1)
                {
                    itemRarity = FavoriteItemRare.R;
                }
                else if (favoriteItem.Tid >= 200101 && favoriteItem.Tid <= 201301 && favoriteItem.Tid % 100 == 1)
                {
                    itemRarity = FavoriteItemRare.SSR;
                }
                
            }
           

            if (itemRarity == FavoriteItemRare.SSR)
            {
                int ssrMaxLevel = 2; // SSR has levels 0, 1, 2
                
                if (favoriteItem.Lv < ssrMaxLevel)
                {
                    favoriteItem.Lv++;
                    favoriteItem.Exp = 0; // SSR items don't use exp system
                }

                if (favoriteItem.Lv >= ssrMaxLevel)
                {
                    favoriteItem.Exp = 0;
                }
                
                return;
            }

            var expRecords = GameData.Instance.FavoriteItemExpTable.Values
                .Where(x => x.FavoriteRare == itemRarity)
                .OrderBy(x => x.Level)
                .ToList();


            if (!expRecords.Any())
            {
                var allRarities = GameData.Instance.FavoriteItemExpTable.Values.Select(x => x.FavoriteRare).Distinct();
                return;
            }

            int maxLevel = 15;

            while (favoriteItem.Lv < maxLevel)
            {
                int expRequired = GetExpRequiredForLevel(favoriteItem.Lv + 1, expRecords);
                
                if (favoriteItem.Exp >= expRequired && expRequired > 0)
                {
                    favoriteItem.Exp -= expRequired;
                    favoriteItem.Lv++;
                }
                else
                {
                    break;
                }
            }

            // Cap excess exp if at max level
            if (favoriteItem.Lv >= maxLevel)
            {
                favoriteItem.Exp = 0;
            }
            
        }

        private int GetExpRequiredForLevel(int level, List<FavoriteItemExpRecord> expRecords)
        {
            var record = expRecords.FirstOrDefault(x => x.Level == level);
            return record?.NeedExp ?? 0;
        }
        
    }
}