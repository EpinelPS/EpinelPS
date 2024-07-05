using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Simroom
{
    [PacketPath("/simroom/get")]
    public class GetSimRoomData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSimRoom>();

            var response = new ResGetSimRoom();
            // TODO
            WriteData(response);
        }
    }
}
