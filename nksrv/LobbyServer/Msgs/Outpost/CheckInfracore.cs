using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Outpost
{
    [PacketPath("/infracore/check")]
    public class CheckInfracore : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckReceiveInfraCoreReward>();
            var response = new ResCheckReceiveInfraCoreReward();

            // TODO

            WriteData(response);
        }
    }
}
