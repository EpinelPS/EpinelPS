using EmbedIO;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Auth
{
    [PacketPath("/auth/enterserver")]
    public class GetUserOnlineStateLog : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<EnterServerRequest>();

            // request has auth token
            UsedAuthToken = req.AuthToken;
            foreach (var item in JsonDb.Instance.LauncherAccessTokens)
            {
                if (item.Token == UsedAuthToken)
                {
                    UserId = item.UserID;
                }
            }
            if (UserId == 0) throw new HttpException(403);

            var user = GetUser();

            var response = new EnterServerResponse();
            var rsp = LobbyHandler.GenGameClientTok(req.ClientPublicKey, req.AuthToken);
            response.GameClientToken = rsp.ClientAuthToken;
            response.FeatureDataInfo = new NetFeatureDataInfo() { UseFeatureData = true };
            response.Identifier = new NetLegacyUserIdentifier() { Server = 21769, Usn = (long)user.ID };
            response.ShouldRestartAfter = Duration.FromTimeSpan(TimeSpan.FromSeconds(86400));

            response.EncryptionToken = ByteString.CopyFromUtf8(rsp.ClientAuthToken);
            await WriteDataAsync(response);
        }
    }
}
