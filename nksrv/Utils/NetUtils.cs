

using nksrv.StaticInfo;
using Swan.Logging;
using System.Reflection;

namespace nksrv.Utils
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
                Lv = item.Level,
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
                        itemDictionary[item.ItemType] = new NetUserItemData() { Count = item.Count, Tid = item.ItemType, Csn = item.Csn, Lv = item.Level, Exp = item.Exp, Corporation = item.Corp, Isn = item.Isn, Position = item.Position };
                    }
                }
                else
                {
                    var newItem = new NetUserItemData() { Count = item.Count, Tid = item.ItemType, Csn = item.Csn, Lv = item.Level, Exp = item.Exp, Corporation = item.Corp, Isn = item.Isn, Position = item.Position };
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
                    var subType = StaticDataParser.Instance.GetItemSubType(item.ItemType);
                    switch(subType)
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
    }
}