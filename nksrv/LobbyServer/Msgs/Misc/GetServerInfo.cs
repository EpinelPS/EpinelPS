using Google.Protobuf.WellKnownTypes;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Misc
{
    [PacketPath("/getserverinfo")]
    public class GetServerInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var r = new ResGetServerInfo();

            // todo: reimplement this as well
            r.MatchUrl = "https://global-match.nikke-kr.com";
            r.WorldId = 84;

          await  WriteDataAsync(r);
        }
    }
}
