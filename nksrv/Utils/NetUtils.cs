
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
    }
}