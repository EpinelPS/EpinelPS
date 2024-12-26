using EpinelPS.Database;
using EpinelPS.StaticInfo;

namespace EpinelPS.Utils
{
    // Calculate rewards for various messages
    public class RewardUtils
    {
        
        public static NetRewardData RegisterRewardsForUser(Database.User user, RewardTableRecord rewardData)
        {
            NetRewardData ret = new();
            ret.PassPoint = new();
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
                if (item.reward_id != 0)
                {
                    if (string.IsNullOrEmpty(item.reward_type) || string.IsNullOrWhiteSpace(item.reward_type)) { }
                    else if (item.reward_type == "Currency")
                    {
                        bool found = false;
                        foreach (var currentReward in user.Currency)
                        {
                            if (currentReward.Key == (CurrencyType)item.reward_id)
                            {
                                user.Currency[currentReward.Key] += item.reward_value;

                                ret.Currency.Add(new NetCurrencyData()
                                {
                                    FinalValue = user.Currency[currentReward.Key],
                                    Value = item.reward_value,
                                    Type = item.reward_id
                                });
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            user.Currency.Add((CurrencyType)item.reward_id, item.reward_value);
                            ret.Currency.Add(new NetCurrencyData()
                            {
                                FinalValue = item.reward_value,
                                Value = item.reward_value,
                                Type = item.reward_id
                            });
                        }
                    }
                    else if (item.reward_type == "Item")
                    {
                        // Check if user already has said item. If it is level 1, increase item count.
                        // If user does not have item, generate a new item ID
                        if (user.Items.Where(x => x.ItemType == item.reward_id && x.Level == 1).Any())
                        {
                            ItemData? newItem = user.Items.Where(x => x.ItemType == item.reward_id && x.Level == 1).FirstOrDefault();
                            if (newItem != null)
                            {
                                newItem.Count += item.reward_value;

                                // Tell the client the reward and its amount
                                ret.Item.Add(new NetItemData()
                                {
                                    Count = item.reward_value,
                                    Tid = item.reward_id,
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
                            user.Items.Add(new ItemData() { ItemType = item.reward_id, Isn = id, Level = 1, Exp = 0, Count = item.reward_value });
                            ret.Item.Add(new NetItemData()
                            {
                                Count = item.reward_value,
                                Tid = item.reward_id,
                                //Isn = id
                            });

                            // Tell the client the new amount of this item (which is the same as user did not have item previously)
                            ret.UserItems.Add(new NetUserItemData()
                            {
                                Isn = id,
                                Tid = item.reward_id,
                                Count = item.reward_value
                            });
                        }
                    }
                    else if (item.reward_type == "Memorial")
                    {
                        if (!user.Memorial.Contains(item.reward_id))
                        {
                            ret.Memorial.Add(item.reward_id);
                            user.Memorial.Add(item.reward_id);
                        }
                    }
                    else if (item.reward_type == "Bgm")
                    {
                        if (!user.JukeboxBgm.Contains(item.reward_id))
                        {
                            ret.JukeboxBgm.Add(item.reward_id);
                            user.JukeboxBgm.Add(item.reward_id);
                        }
                    }
                    else
                    {
                        Console.WriteLine("TODO: Reward type " + item.reward_type);
                    }
                }
            }

            return ret;
        }
    }
}
