using Google.Protobuf;
using nksrv.StaticInfo;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Misc
{
    [PacketPath("/featureflags/get")]
    public class GetFeatureFlags : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetFeatureFlag>();

            var r = new ResGetFeatureFlag();
            r.IsOpen = true;

            WriteData(r);
        }
    }
}
