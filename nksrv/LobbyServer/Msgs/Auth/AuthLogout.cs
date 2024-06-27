using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Auth
{
    [PacketPath("/auth/logout")]
    public class AuthLogout : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqLogout>();

            JsonDb.Instance.GameClientTokens.Remove(UsedAuthToken);
            WriteData(new ResLogout());
        }
    }
}
