using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Event
{
    [PacketPath("/event/list")]
    public class ListEvents : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventList>();

            var response = new ResGetEventList();

            // TODO: Support events



            response.EventList.Add(new NetEventData()
            {
                Id = 100016,
                EventDisableDate = 1000000000000000,
                EventStartDate = 1,
                EventEndDate = 1000000000000000,
                EventVisibleDate = 0,
                EventSystemType = 9
            });

            response.EventList.Add(new NetEventData()
            {
                Id = 81400,
                EventDisableDate = 638574479990000000,
                EventStartDate = 638556192000000000,
                EventEndDate = 638574479990000000,
                EventVisibleDate = 638556192000000000,
                EventSystemType = 10
            });

            // Island Adventure
            response.EventList.Add(new NetEventData()
            {
                Id = 81401,
                EventStartDate = 638556192000000000,
                EventVisibleDate = 638556192000000000,
                EventDisableDate = 638574479990000000,
                EventEndDate = 638574479990000000,
                EventSystemType = 35
            });

            // Aegis the Diver event
            response.EventList.Add(new NetEventData()
            {
                Id = 800001,
                EventSystemType = 36,
                EventVisibleDate = 0,
                EventStartDate = 0,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });
            WriteData(response);
        }
    }
}
