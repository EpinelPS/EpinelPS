using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/useselectbox")]
    public class UseSelectBox : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "isn": "92999919299991", "select": [ { "id": 1, "count": 5 }, { "id": 2, "count": 5 } ] }
            ReqUseSelectBox req = await ReadData<ReqUseSelectBox>();
            User user = GetUser();

            ResUseSelectBox response = new();

            // calculate total item count
            int itemCount = req.Select.Sum(x => x.Count);
            if (itemCount <= 0) throw new Exception("no items selected");

            // find box in inventory
            ItemData box = user.Items.Where(x => x.Isn == req.Isn).FirstOrDefault() ?? throw new InvalidDataException("cannot find box with isn " + req.Isn);
            if (itemCount > box.Count) throw new Exception("count mismatch");



            // find matching consumable entry
            ItemConsumeRecord? itemConsume = GameData.Instance.ConsumableItems.Values.Where(x => x.id == box.ItemType).FirstOrDefault() ?? throw new Exception($"cannot find any consumable item entries with ID {box.ItemType}");
            Logging.WriteLine($"UseSelectBox {box.ItemType}: Found item consume - {JsonConvert.SerializeObject(itemConsume)}", LogType.Debug);

            NetRewardData reward = new()
            {
                PassPoint = new() { }
            };

            // find matching select options entry
            if (GameData.Instance.ItemSelectOptionTable.TryGetValue(itemConsume.use_id, out ItemSelectOption? selectOptions))
            {
                foreach (var selected in req.Select)
                {
                    // find matching option
                    SelectOption option = selectOptions.select_option[selected.Id] ?? throw new Exception($"invalid selection ID {selected.Id}");
                    Logging.WriteLine($"UseSelectBox {box.ItemType}: Found selected option - {JsonConvert.SerializeObject(option)}", LogType.Debug);

                    for (int i = 0; i < selected.Count; i++)
                    {
                        if (option.select_type.Equals("Currency"))
                        {
                            RewardUtils.AddSingleCurrencyObject(user, ref reward, (CurrencyType)option.select_id, option.select_value);
                        }
                        else
                        {
                            RewardUtils.AddSingleObject(user, ref reward, option.select_id, option.select_type, option.select_value);
                        }
                    }

                }
            }
            JsonDb.Save();
            // update client side box count
            box.Count -= itemCount;
            if (box.Count == 0) user.Items.Remove(box);
            reward.UserItems.Add(NetUtils.UserItemDataToNet(box));
            response.Reward = reward;
            await WriteDataAsync(response);
        }

    }
}
