using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/usetimereward")]
    public class UseTimeReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            /*
             * Req Contains:
             * Isn: long value
             * Count: int value, how many items to use
            */
            ReqUseTimeReward req = await ReadData<ReqUseTimeReward>();
            User user = GetUser();
            ResUseTimeReward response = new();

            ItemData timeReward = user.Items.Where(x => x.Isn == req.Isn).FirstOrDefault() ?? throw new InvalidDataException("cannot find time reward with isn " + req.Isn);
            if (req.Count > timeReward.Count) throw new Exception("count mismatch");

            timeReward.Count -= req.Count;
            if (timeReward.Count == 0) user.Items.Remove(timeReward);

            ItemConsumeRecord? cItem = GameData.Instance.ConsumableItems
                .FirstOrDefault(x => x.Value.id == timeReward.ItemType).Value
                ?? throw new Exception("cannot find box id " + timeReward.ItemType);

            // TODO: find out where these numbers come from
            (CurrencyType itemType, long amount) = cItem.use_id switch
            {
                1 => (CurrencyType.Gold, NetUtils.GetOutpostRewardAmount(user, CurrencyType.Gold, TimeSpan.FromSeconds(cItem.use_value).TotalMinutes, false)),
                2 => (CurrencyType.CharacterExp, NetUtils.GetOutpostRewardAmount(user, CurrencyType.CharacterExp, TimeSpan.FromSeconds(cItem.use_value).TotalMinutes, false)),
                4 => (CurrencyType.CharacterExp2, NetUtils.GetOutpostRewardAmount(user, CurrencyType.CharacterExp2, TimeSpan.FromSeconds(cItem.use_value).TotalMinutes, false)),
                _ => throw new Exception("unknown use_id " + cItem.use_id)
            };

            NetRewardData reward = new();
            RewardUtils.AddSingleCurrencyObject(user, ref reward, itemType, amount);

            response.Reward = reward;
            // update client side item count
            response.Reward.UserItems.Add(NetUtils.UserItemDataToNet(timeReward));

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
