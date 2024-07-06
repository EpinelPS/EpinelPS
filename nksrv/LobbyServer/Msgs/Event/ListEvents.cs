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

            response.EventList.Add(new NetEventData()
            {
                Id = 81301,
                EventDisableDate = 1000000000000000,
                EventStartDate = 1,
                EventEndDate = 1000000000000000,
                EventVisibleDate = 0,
                EventSystemType = 34
            });


            // TODO: Support events

            response.EventList.Add(new NetEventData()
            {
                Id = 20001,
                EventDisableDate = 1000000000000000,
                EventStartDate = 1,
                EventEndDate = 1000000000000000,
                EventVisibleDate = 0,
                EventSystemType = 1
            });





            // Island Adventure
            response.EventList.Add(new NetEventData()
            {
                Id = 81400,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 10
            });
            response.EventList.Add(new NetEventData()
            {
                Id = 81401,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 35
            });
            response.EventList.Add(new NetEventData()
            {
                Id = 81402,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 11
            });

            response.EventList.Add(new NetEventData()
            {
                Id = 81403,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 2
            });

            response.EventList.Add(new NetEventData()
            {
                Id = 160011,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventSystemType = 2
            });

            // Aegis the Diver event
            response.EventList.Add(new NetEventData()
            {
                Id = 800001,
                EventSystemType = 36,
                EventVisibleDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventStartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                EventEndDate = DateTime.Now.AddDays(20).Ticks,
                EventDisableDate = DateTime.Now.AddDays(20).Ticks,
            });
            WriteData(response);
        }
    }
}
