using EpinelPS.Database;
using EpinelPS.StaticInfo;
using Swan.Logging;

namespace EpinelPS.Utils
{
    public class NetUtils
    {
        public static NetUserItemData ToNet(ItemData item)
        {
            return new()
            {
                Corporation = item.Corp,
                Count = item.Count,
                Csn = item.Csn,
                Exp = item.Exp,
                Isn = item.Isn,
                Level = item.Level,
                Position = item.Position,
                Tid = item.ItemType
            };
        }

        public static List<NetUserItemData> GetUserItems(User user)
        {
            List<NetUserItemData> ret = new();
            Dictionary<int, NetUserItemData> itemDictionary = new Dictionary<int, NetUserItemData>();

            foreach (var item in user.Items.ToList())
            {
                if (item.Csn == 0)
                {
                    if (itemDictionary.ContainsKey(item.ItemType))
                    {
                        itemDictionary[item.ItemType].Count++;
                    }
                    else
                    {
                        itemDictionary[item.ItemType] = new NetUserItemData() { Count = item.Count, Tid = item.ItemType, Csn = item.Csn, Level = item.Level, Exp = item.Exp, Corporation = item.Corp, Isn = item.Isn, Position = item.Position };
                    }
                }
                else
                {
                    var newItem = new NetUserItemData() { Count = item.Count, Tid = item.ItemType, Csn = item.Csn, Level = item.Level, Exp = item.Exp, Corporation = item.Corp, Isn = item.Isn, Position = item.Position };
                    itemDictionary[item.ItemType] = newItem;
                }
            }

            return ret;
        }

        public static int GetItemPos(User user, long isn)
        {
            foreach (var item in user.Items)
            {
                if (item.Isn == isn)
                {
                    var subType = GameData.Instance.GetItemSubType(item.ItemType);
                    switch (subType)
                    {
                        case "Module_A":
                            return 0;
                        case "Module_B":
                            return 1;
                        case "Module_C":
                            return 2;
                        case "Module_D":
                            return 3;
                        default:
                            Logger.Warn("Unknown item subtype: " + subType);
                            break;
                    }
                    break;
                }
            }

            return 0;
        }
        /// <summary>
        /// Takes multiple NetRewardData objects and merges it into one. Note that this function expects that rewards are already applied to user object.
        /// </summary>
        /// <param name="rewards">list of rewards</param>
        /// <param name="user">user to pull old currency count from</param>
        /// <returns></returns>
        public static NetRewardData MergeRewards(List<NetRewardData> rewards, User user)
        {
            NetRewardData result = new();

            Dictionary<int, long> currencyDict = new Dictionary<int, long>();
            List<NetItemData> items = new List<NetItemData>();

            foreach (NetRewardData reward in rewards)
            {
                foreach (var c in reward.Currency)
                {
                    if (currencyDict.ContainsKey(c.Type))
                        currencyDict[c.Type] += c.Value;
                    else
                        currencyDict.Add(c.Type, c.Value);
                }

                foreach (var item in reward.Item)
                {
                    items.Add(item);
                }

                foreach (var c in reward.Character)
                {
                    Logger.Warn("MergeRewards - TODO Character");
                }
            }

            foreach (var c in currencyDict)
            {
                result.Currency.Add(new NetCurrencyData() { Value = c.Value, Type = c.Key, FinalValue = user.Currency[(CurrencyType)c.Key] });
            }

            // TODO is this right?
            foreach (var c in items)
            {
                result.Item.Add(c);
            }
            return result;
        }
    }
}