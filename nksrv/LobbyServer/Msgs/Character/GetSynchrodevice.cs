using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Character
{
    [PacketPath("/character/synchrodevice/get")]
    public class GetSynchrodevice : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSynchroData>();

            var response = new ReqGetSynchroData();
            response.Synchro = new NetUserSynchroData();
            response.Synchro.SynchroLv = 1;
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 1 });
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 2 });
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 3 });
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 4 });
            response.Synchro.Slots.Add(new NetSynchroSlot() { Slot = 5 });
            // TODO: Validate response from real server and pull info from user info
            WriteData(response);
        }
    }
}
