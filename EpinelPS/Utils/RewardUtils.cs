using EpinelPS.Data;
using EpinelPS.Database;
using Org.BouncyCastle.Ocsp;

namespace EpinelPS.Utils
{
    // Calculate rewards for various messages
    public class RewardUtils
    {
        public static NetRewardData RegisterRewardsForUser(User user, int rewardId)
        {
            RewardRecord rewardData = GameData.Instance.GetRewardTableEntry(rewardId) ?? throw new Exception($"unknown reward Id {rewardId}");
            return RegisterRewardsForUser(user, rewardData);
        }
        public static NetRewardData RegisterRewardsForUser(User user, RewardRecord rewardData)
        {
            NetRewardData ret = new()
            {
                PassPoint = new()
            };
            if (rewardData.Rewards == null) return ret;

            if (rewardData.UserExp != 0)
            {
                int newXp = rewardData.UserExp + user.userPointData.ExperiencePoint;

                int newLevelExp = GameData.Instance.GetUserMinXpForLevel(user.userPointData.UserLevel);
                int newLevel = user.userPointData.UserLevel;

                if (newLevelExp == -1)
                {
                    Console.WriteLine("Unknown user level value for xp " + newXp);
                }

                int newGems = 0;

                while (newXp >= newLevelExp)
                {
                    newLevel++;
                    newGems += 30;
                    newXp -= newLevelExp;
                    if (user.Currency.ContainsKey(CurrencyType.FreeCash))
                        user.Currency[CurrencyType.FreeCash] += 30;
                    else
                        user.Currency.Add(CurrencyType.FreeCash, 30);

                    newLevelExp = GameData.Instance.GetUserMinXpForLevel(newLevel);
                }


                // TODO: what is the difference between IncreaseExp and GainExp
                // NOTE: Current Exp/Lv refers to after XP was added.

                ret.UserExp = new NetIncreaseExpData()
                {
                    BeforeExp = user.userPointData.ExperiencePoint,
                    BeforeLv = user.userPointData.UserLevel,

                    // IncreaseExp = rewardData.UserExp,
                    CurrentExp = newXp,
                    CurrentLv = newLevel,

                    GainExp = rewardData.UserExp,

                };
                user.userPointData.ExperiencePoint = newXp;

                user.userPointData.UserLevel = newLevel;
            }

            foreach (var item in rewardData.Rewards)
            {
                if (item.RewardType != RewardType.None)
                {
                    if (item.RewardPercent != 1000000)
                    {
                        Logging.WriteLine("WARNING: ignoring percent: " + item.RewardPercent / 10000.0 + ", item will be added anyways", LogType.Warning);
                    }

                    AddSingleObject(user, ref ret, item.RewardId, item.RewardType, item.RewardValue);
                }
            }

            return ret;
        }
        public static void AddSingleCurrencyObject(User user, ref NetRewardData ret, CurrencyType currencyType, long rewardCount)
        {
            bool found = user.Currency.Any(pair => pair.Key == currencyType);

            if (found)
            {
                user.Currency[currencyType] += rewardCount;
            }
            else
            {
                user.Currency.Add(currencyType, rewardCount);
            }
            ret.Currency.Add(new NetCurrencyData()
            {
                FinalValue = found ? user.GetCurrencyVal(currencyType) : rewardCount,
                Value = rewardCount,
                Type = (int)currencyType
            });
        }
        /// <summary>
        /// Adds a single item to users inventory, and also adds it to ret parameter.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ret"></param>
        /// <param name="rewardId"></param>
        /// <param name="rewardType"></param>
        /// <param name="rewardCount"></param>
        /// <exception cref="Exception"></exception>
        public static void AddSingleObject(User user, ref NetRewardData ret, int rewardId, RewardType rewardType, int rewardCount)
        {
            if (rewardId == 0 || rewardType == RewardType.None) return;

            if (rewardType == RewardType.Currency)
            {
                AddSingleCurrencyObject(user, ref ret, (CurrencyType)rewardId, rewardCount);
            }
            else if (rewardType == RewardType.Item || 
                rewardType.ToString().StartsWith("Equipment_"))
            {
                // Check if user already has saId item. If it is level 1, increase item count.
                // If user does not have item, generate a new item ID
                if (user.Items.Where(x => x.ItemType == rewardId && x.Level == 1).Any())
                {
                    ItemData? newItem = user.Items.Where(x => x.ItemType == rewardId && x.Level == 1).FirstOrDefault();
                    if (newItem != null)
                    {
                        newItem.Count += rewardCount;

                        // Tell the client the reward and its amount
                        ret.Item.Add(new NetItemData()
                        {
                            Count = rewardCount,
                            Tid = rewardId,
                            //Isn = newItem.Isn
                        });

                        // Tell the client the new amount of this item
                        ret.UserItems.Add(new NetUserItemData()
                        {
                            Isn = newItem.Isn,
                            Tid = newItem.ItemType,
                            Count = newItem.Count
                        });
                    }
                    else
                    {
                        throw new Exception("should not occur");
                    }
                }
                else
                {

                    int Id = user.GenerateUniqueItemId();
                    user.Items.Add(new ItemData() { ItemType = rewardId, Isn = Id, Level = 1, Exp = 0, Count = rewardCount });
                    ret.Item.Add(new NetItemData()
                    {
                        Count = rewardCount,
                        Tid = rewardId,
                        //Isn = Id
                    });

                    // Tell the client the new amount of this item (which is the same as user dId not have item previously)
                    ret.UserItems.Add(new NetUserItemData()
                    {
                        Isn = Id,
                        Tid = rewardId,
                        Count = rewardCount
                    });
                }
            }
            else if (rewardType == RewardType.Memorial)
            {
                if (!user.Memorial.Contains(rewardId))
                {
                    ret.Memorial.Add(rewardId);
                    user.Memorial.Add(rewardId);
                }
            }
            else if (rewardType == RewardType.Bgm)
            {
                if (!user.JukeboxBgm.Contains(rewardId))
                {
                    ret.JukeboxBgm.Add(rewardId);
                    user.JukeboxBgm.Add(rewardId);
                }
            }
            else if (rewardType == RewardType.InfraCoreExp)
            {
                int beforeLv = user.InfraCoreLvl;
                int beforeExp = user.InfraCoreExp;

                user.InfraCoreExp += rewardCount;

                // Check for level ups
                Dictionary<int, InfraCoreGradeRecord> gradeTable = GameData.Instance.InfracoreTable;
                int newLevel = user.InfraCoreLvl;

                foreach (var grade in gradeTable.Values.OrderBy(g => g.Grade))
                {
                    if (user.InfraCoreExp >= grade.InfraCoreExp)
                    {
                        newLevel = grade.Grade + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (newLevel > user.InfraCoreLvl)
                {
                    user.InfraCoreLvl = newLevel;
                }

                ret.InfraCoreExp = new NetIncreaseExpData()
                {
                    BeforeLv = beforeLv,
                    BeforeExp = beforeExp,
                    CurrentLv = user.InfraCoreLvl,
                    CurrentExp = user.InfraCoreExp,
                    GainExp = rewardCount
                };
            }
            else if (rewardType == RewardType.ItemRandomBox)
            {
                ItemConsumeRecord? cItem = GameData.Instance.ConsumableItems.Where(x => x.Value.Id == rewardId).FirstOrDefault().Value;

                if (cItem.ItemSubType == ItemSubType.ItemRandomBoxList)
                {
                    NetRewardData reward = NetUtils.UseLootBox(user, rewardId, rewardCount);

                    ret = NetUtils.MergeRewards([ret, reward], user);
                }
                else
                {
                    NetItemData itm = new()
                    {
                        Count = rewardCount,
                        Tid = cItem.Id,
                        Isn = user.GenerateUniqueItemId()
                    };
                    ret.Item.Add(itm);

                    user.Items.Add(new ItemData() { Count = rewardCount, Isn = itm.Isn, ItemType = itm.Tid });
                }
            }
            else if (rewardType == RewardType.FavoriteItem)
            {

                NetUserFavoriteItemData newFavoriteItem = new NetUserFavoriteItemData
                {
                    FavoriteItemId = user.GenerateUniqueItemId(),
                    Tid = rewardId,
                    Csn = 0,
                    Lv = 0,
                    Exp = 0
                };
                user.FavoriteItems.Add(newFavoriteItem);

                ret.UserFavoriteItems.Add(newFavoriteItem);

                NetFavoriteItemData favoriteItemData = new NetFavoriteItemData
                {
                    FavoriteItemId = newFavoriteItem.FavoriteItemId,
                    Tid = newFavoriteItem.Tid,
                    Csn = newFavoriteItem.Csn,
                    Lv = newFavoriteItem.Lv,
                    Exp = newFavoriteItem.Exp
                };
                ret.FavoriteItems.Add(favoriteItemData);

            }
            else
            {
                Logging.WriteLine("TODO: Reward type " + rewardType, LogType.Warning);
            }
        }
    }
}
