
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using System.Text.Json;

namespace EpinelPS.LobbyServer.Inventory;

[GameRequest("/inventory/quickuseitem")]
public class QuickUseItem : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqQuickUseItem req = await ReadData<ReqQuickUseItem>();
        User user = GetUser();
        ResQuickUseItem response = new();

        List<NetQuickUseData> quickUseItems = req.QuickUse.ToList();
        List<NetUserItemData> rewardUserItems = [];
        List<NetRewardData> rewards = [];

        foreach (var item in quickUseItems)
        {

            DbItemData itemInInventory = user.Items.Where(x => x.Isn == item.Isn).FirstOrDefault()
                ?? throw new InvalidDataException("cannot find item with isn " + item.Isn);

            var totalItems = item.Count;
            if (totalItems > itemInInventory.Count)
                throw new Exception("count mismatch");

            itemInInventory.Count -= totalItems;
            if (itemInInventory.Count == 0) user.Items.Remove(itemInInventory);
            var cItem = GameData.Instance.ConsumableItems
            .Where(x => x.Value.Id == itemInInventory.ItemType).FirstOrDefault().Value
            ?? throw new Exception("cannot find BundleBox Id " + itemInInventory.Isn);
            Console.WriteLine($"Item: {item.Isn}, type {cItem.UseType} and quickuseItemType {cItem.QuickUseType},\n count {item.Count}/{itemInInventory.Count}");
            switch (cItem.UseType)
            {
                case ItemUseType.None:
                    break;
                case ItemUseType.BundleBox:
                    rewards.Add(NetUtils.UseBundleBox(user, itemInInventory.ItemType, totalItems));
                    rewardUserItems.Add(NetUtils.UserItemDataToNet(itemInInventory));
                    break;
                case ItemUseType.ItemRandomBox:
                    rewards.Add(NetUtils.UseLootBox(user, itemInInventory.ItemType, totalItems));
                    rewardUserItems.Add(NetUtils.UserItemDataToNet(itemInInventory));
                    break;
                default:
                    break;
            }
        }

        var reward = new NetRewardData();
        rewards.ForEach(x => reward.MergeFrom(x));
        response.Reward = reward;
        response.Reward.UserItems.Add(rewardUserItems);
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}