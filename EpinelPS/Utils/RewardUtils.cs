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
            var rewardData = GameData.Instance.GetRewardTableEntry(rewardId) ?? throw new Exception($"unknown reward id {rewardId}");
            return RegisterRewardsForUser(user, rewardData);
        }
        public static NetRewardData RegisterRewardsForUser(User user, RewardTableRecord rewardData)
        {
            NetRewardData ret = new()
            {
                PassPoint = new()
            };
            if (rewardData.rewards == null) return ret;

            if (rewardData.user_exp != 0)
            {
                var newXp = rewardData.user_exp + user.userPointData.ExperiencePoint;

                var oldXpData = GameData.Instance.GetUserLevelFromUserExp(user.userPointData.ExperiencePoint);
                var newLevelExp = GameData.Instance.GetUserMinXpForLevel(user.userPointData.UserLevel);
                var newLevel = user.userPointData.UserLevel;

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

                    // IncreaseExp = rewardData.user_exp,
                    CurrentExp = newXp,
                    CurrentLv = newLevel,

                    GainExp = rewardData.user_exp,

                };
                user.userPointData.ExperiencePoint = newXp;

                user.userPointData.UserLevel = newLevel;
            }

            foreach (var item in rewardData.rewards)
            {
                if (!string.IsNullOrEmpty(item.reward_type))
                {
                    if (item.reward_percent != 1000000)
                    {
                        Logging.WriteLine("WARNING: ignoring percent: " + item.reward_percent / 10000.0 + ", item will be added anyways", LogType.Warning);
                    }

                    AddSingleObject(user, ref ret, item.reward_id, item.reward_type, item.reward_value);
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
                FinalValue = found ? user.Currency[currencyType] + rewardCount : rewardCount,
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
        public static void AddSingleObject(User user, ref NetRewardData ret, int rewardId, string rewardType, int rewardCount)
        {
            if (rewardId != 0 || !string.IsNullOrEmpty(rewardType))
            {
                if (string.IsNullOrEmpty(rewardType) || string.IsNullOrWhiteSpace(rewardType)) { }
                else if (rewardType == "Item")
                {
                    // Check if user already has said item. If it is level 1, increase item count.
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

                        var id = user.GenerateUniqueItemId();
                        user.Items.Add(new ItemData() { ItemType = rewardId, Isn = id, Level = 1, Exp = 0, Count = rewardCount });
                        ret.Item.Add(new NetItemData()
                        {
                            Count = rewardCount,
                            Tid = rewardId,
                            //Isn = id
                        });

                        // Tell the client the new amount of this item (which is the same as user did not have item previously)
                        ret.UserItems.Add(new NetUserItemData()
                        {
                            Isn = id,
                            Tid = rewardId,
                            Count = rewardCount
                        });
                    }
                }
                else if (rewardType == "Memorial")
                {
                    if (!user.Memorial.Contains(rewardId))
                    {
                        ret.Memorial.Add(rewardId);
                        user.Memorial.Add(rewardId);
                    }
                }
                else if (rewardType == "Bgm")
                {
                    if (!user.JukeboxBgm.Contains(rewardId))
                    {
                        ret.JukeboxBgm.Add(rewardId);
                        user.JukeboxBgm.Add(rewardId);
                    }
                }
                else if (rewardType == "InfraCoreExp")
                {
                    ret.InfraCoreExp = new NetIncreaseExpData()
                    {
                        BeforeLv = user.InfraCoreLvl,
                        BeforeExp = user.InfraCoreExp,
                        // TODO
                    };
                }
                else if (rewardType == "ItemRandomBox")
                {
                    ItemConsumeRecord? cItem = GameData.Instance.ConsumableItems.Where(x => x.Value.id == rewardId).FirstOrDefault().Value;

                    if (cItem.item_sub_type == ItemSubType.ItemRandomBoxList)
                    {
                        NetRewardData reward = NetUtils.UseLootBox(user, rewardId, rewardCount);

                        NetUtils.RegisterRewardsForUser(user, reward);
                        ret = NetUtils.MergeRewards([ret, reward], user);
                    }
                    else
                    {
                        var itm = new NetItemData()
                        {
                            Count = rewardCount,
                            Tid = cItem.id,
                            Isn = user.GenerateUniqueItemId()
                        };
                        ret.Item.Add(itm);
                        user.Items.Add(new ItemData() { Count = rewardCount, Isn = itm.Isn, ItemType = itm.Tid });
                    }
                }
                else
                {
                    Logging.WriteLine("TODO: Reward type " + rewardType, LogType.Warning);
                }
            }
        }
    }
}
