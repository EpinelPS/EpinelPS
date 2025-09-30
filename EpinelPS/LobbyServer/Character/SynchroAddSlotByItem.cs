using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/synchrodevice/addslotbyitem")]
    public class SynchroAddSlotByItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Broken protocol so we dIdn't validate request data.
            // May fix later.
            ReqSynchroAddSlot req = await ReadData<ReqSynchroAddSlot>();
            
            User user = GetUser();
            ResSynchroAddSlot response = new();

            NetSynchroSlot newSlot = new()
            {
                Csn = 0,
                Slot = user.SynchroSlots.Last().Slot + 1, // any upper bound?
                AvailableRegisterAt = 0
            };

            user.SynchroSlots.Add(new SynchroSlot()
            {
                Slot = newSlot.Slot,
                CharacterSerialNumber = newSlot.Csn,
                AvailableAt = newSlot.AvailableRegisterAt
            });

            response.Slot = newSlot;

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
