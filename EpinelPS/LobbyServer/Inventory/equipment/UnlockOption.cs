using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Inventory;

[GameRequest("/inventory/equipment/unlockoption")]
public class UnlockOption : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAwakeningUnlockOption req = await ReadData<ReqAwakeningUnlockOption>();
        User user = GetUser();

        ResAwakeningUnlockOption response = new();

        if (req.Isn <= 0 || req.Slot < 1 || req.Slot > 3)
        {
            await WriteDataAsync(response);
            return;
        }

        EquipmentAwakeningData? awakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn && !x.IsNewData)
            ?? user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == req.Isn);

        if (awakening == null)
        {
            await WriteDataAsync(response);
            return;
        }

        switch (req.Slot)
        {
            case 1:
                awakening.Option.Option1Lock = false;
                awakening.Option.IsOption1DisposableLock = false;
                break;
            case 2:
                awakening.Option.Option2Lock = false;
                awakening.Option.IsOption2DisposableLock = false;
                break;
            case 3:
                awakening.Option.Option3Lock = false;
                awakening.Option.IsOption3DisposableLock = false;
                break;
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
