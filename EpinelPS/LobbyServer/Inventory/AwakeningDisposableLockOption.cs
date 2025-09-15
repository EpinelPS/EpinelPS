using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/equipment/lockoption/disposable")]
    public class AwakeningDisposableLockOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAwakeningDisposableLockOption req = await ReadData<ReqAwakeningDisposableLockOption>();
            User user = GetUser();

            ResAwakeningDisposableLockOption response = new();

            if (user.EquipmentAwakeningOptions.TryGetValue(req.Isn, out AwakeningOption? awakeningOption))
            {
                if (awakeningOption.Option1Id == req.OptionId)
                {
                    user.EquipmentAwakeningOptions[req.Isn].IsOption1DisposableLock = req.IsLocked;
                }
                if (awakeningOption.Option2Id == req.OptionId)
                {
                    user.EquipmentAwakeningOptions[req.Isn].IsOption2DisposableLock = req.IsLocked;
                }
                if (awakeningOption.Option3Id == req.OptionId)
                {
                    user.EquipmentAwakeningOptions[req.Isn].IsOption3DisposableLock = req.IsLocked;
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