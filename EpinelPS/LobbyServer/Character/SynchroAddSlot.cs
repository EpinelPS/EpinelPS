using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/synchrodevice/addslot")]
    public class SynchroAddSlot : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSynchroAddSlot>();
            var user = GetUser();
            var response = new ResSynchroAddSlot();

            var slot = user.SynchroSlots.FirstOrDefault(x => x.Slot == req.Slot);
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
                var newSlot = new NetSynchroSlot
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
}
