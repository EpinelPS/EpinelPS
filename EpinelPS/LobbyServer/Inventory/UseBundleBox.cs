using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/usebundlebox")]
    public class UseBundleBox : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "isn": "92999919299991", "count": 1 }
            ReqUseBundleBox req = await ReadData<ReqUseBundleBox>();
            User user = GetUser();

            ResUseBundleBox response = new();

            // find box in inventory
            ItemData box = user.Items.Where(x => x.Isn == req.Isn).FirstOrDefault() ?? throw new InvalidDataException("cannot find box with isn " + req.Isn);
            if (req.Count > box.Count) throw new Exception("count mismatch");

            // find matching consumable entry
            ItemConsumeRecord? itemConsume = GameData.Instance.ConsumableItems.Values.Where(x => x.id == box.ItemType).FirstOrDefault() ?? throw new Exception($"cannot find any consumable item entries with ID {box.ItemType}");
            Logging.WriteLine($"UseSelectBox {box.ItemType}: Found item consume - {JsonConvert.SerializeObject(itemConsume)}", LogType.Debug);

            NetRewardData reward = new()
            {
                PassPoint = new() { }
            };


            // find matching bundle box entry
            if (GameData.Instance.BundleBoxTable.TryGetValue(itemConsume.use_id, out BundleBox? bundleBox))
            {
                // grant all items in the bundle box
                foreach (var item in bundleBox.rewards)
                {
                    if (item.reward_type.Equals("Item"))
                    {
                        RewardUtils.AddSingleObject(user, ref reward, item.reward_id, item.reward_type, item.reward_value * req.Count);
                    }
                    else if (item.reward_type.Equals("Currency"))
                    {
                        RewardUtils.AddSingleCurrencyObject(user, ref reward, (CurrencyType)item.reward_id, item.reward_value * req.Count);
                    }
                    else if (item.reward_id != 0)
                    {
                        Logging.WriteLine($"UseBundleBox {box.ItemType}: unsupported item - {JsonConvert.SerializeObject(item)}", LogType.Debug);
                    }
                }
            }
            else
            {
                Logging.WriteLine($"UseBundleBox {box.ItemType}: cannot find bundle box with ID {itemConsume.use_id}", LogType.Warning);
            }

            // update box count
            box.Count -= req.Count;
            if (box.Count == 0) user.Items.Remove(box);
            JsonDb.Save();

            // update client side box count
            reward.UserItems.Add(NetUtils.UserItemDataToNet(box));

            response.Reward = reward;
            await WriteDataAsync(response);
        }
    }
}
