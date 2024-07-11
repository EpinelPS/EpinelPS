using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Event
{
    [PacketPath("/event/mission/getclear")]
    public class GetClearedMissions : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetEventMissionClear>();

            var response = new ResGetEventMissionClear();
            // TODO
          await  WriteDataAsync(response);
        }
    }
}
