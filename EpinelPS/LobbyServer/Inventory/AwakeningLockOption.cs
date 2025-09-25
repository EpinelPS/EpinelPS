using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/lockoption")]
    public class AwakeningLockOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqAwakeningLockOption = { "isn": "189013410901001", "optionId": 7000814, "isLocked": true }
            ReqAwakeningLockOption req = await ReadData<ReqAwakeningLockOption>();
            User user = GetUser();

            ResAwakeningLockOption response = new();

            if (user.EquipmentAwakeningOptions.TryGetValue(req.Isn, out AwakeningOption? awakeningOption))
            {
                if (awakeningOption.Option1Id == req.OptionId)
                {
                    user.EquipmentAwakeningOptions[req.Isn].Option1Lock = req.IsLocked;
                }
                if (awakeningOption.Option2Id == req.OptionId)
                {
                    user.EquipmentAwakeningOptions[req.Isn].Option2Lock = req.IsLocked;
                }
                if (awakeningOption.Option3Id == req.OptionId)
                {
                    user.EquipmentAwakeningOptions[req.Isn].Option3Lock = req.IsLocked;
                }
            }

            ItemData item = user.Items.FirstOrDefault(x => x.Isn == req.Isn, new ItemData());
            response.Items.Add(NetUtils.ToNet(item));
            response.Currencies.Add(new NetUserCurrencyData() { });
            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}