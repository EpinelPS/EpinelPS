using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/synchrodevice/addslot")]
public class SynchroAddSlot : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSynchroAddSlot req = await ReadData<ReqSynchroAddSlot>();
        User user = GetUser();
        ResSynchroAddSlot response = new();

        SynchroSlot? slot = user.SynchroSlots.FirstOrDefault(x => x.Slot == req.Slot);
        if (slot != null)
        {
            response.Slot = new NetSynchroSlot
            {
                Csn = slot.CharacterSerialNumber,
                Slot = slot.Slot,
                AvailableRegisterAt = slot.AvailableAt
            };
        }
        else
        {
            NetSynchroSlot newSlot = new()
            {
                Csn = 0,
                Slot = req.Slot,
                AvailableRegisterAt = 0
            };

            user.SynchroSlots.Add(new SynchroSlot()
            {
                Slot = newSlot.Slot,
                CharacterSerialNumber = newSlot.Csn,
                AvailableAt = newSlot.AvailableRegisterAt
            });

            response.Slot = newSlot;
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
