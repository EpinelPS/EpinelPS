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
    [PacketPath("/auth/enterserver")]
    public class GetUserOnlineStateLog : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<EnterServerRequest>();

            var response = new EnterServerResponse();
            var rsp = LobbyHandler.GenGameClientTok(req.ClientPublicKey, req.AuthToken);
            response.GameClientToken = rsp.ClientAuthToken;
            response.FeatureDataInfo = new NetFeatureDataInfo() { UseFeatureData = true };
            response.Identifier = new NetLegacyUserIdentifier() { Server = 21769, Usn = 10984769 };
            response.ShouldRestartAfter = Duration.FromTimeSpan(TimeSpan.FromSeconds(86400));

            // This was probably the public key for communication at some point during the game's development
            // But, the developers chose to hardcode server public key in the client, which prevents this
            // private server from "just working", so thats why hex patch is required.
            // The only point of encrypting packets is to make it harder for me to develop this and users to use this.
            response.EncryptionToken = ByteString.CopyFromUtf8(rsp.ClientAuthToken);
            WriteData(response);
        }
    }
}
