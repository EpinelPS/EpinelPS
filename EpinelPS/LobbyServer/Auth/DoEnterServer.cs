using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Paseto.Builder;
using Paseto;
using System.Text.Json;

namespace EpinelPS.LobbyServer.Auth
{
    [PacketPath("/auth/enterserver")]
    public class GetUserOnlineStateLog : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterServer req = await ReadData<ReqEnterServer>();

            // request has auth token
            UsedAuthToken = req.AuthToken;
            foreach (AccessToken item in JsonDb.Instance.LauncherAccessTokens)
            {
                if (item.Token == UsedAuthToken)
                {
                    UserId = item.UserID;
                }
            }
            if (UserId == 0) throw new BadHttpRequestException("unknown auth token", 403);
            User user = GetUser();

            GameClientInfo rsp = LobbyHandler.GenGameClientTok(req.ClientPublicKey, UserId);

            string token = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                               .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                               .AddClaim("userId", UserId)
                               .IssuedAt(DateTime.UtcNow)
                               .Expiration(DateTime.UtcNow.AddDays(2))
                               .Encode();

            string encryptionToken = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                               .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                               .AddClaim("data", JsonSerializer.Serialize(rsp))
                               .IssuedAt(DateTime.UtcNow)
                               .Expiration(DateTime.UtcNow.AddDays(2))
                               .Encode();


            ResEnterServer response = new()
            {
                GameClientToken = token,
                FeatureDataInfo = new NetFeatureDataInfo() { }, // TODO
                Identifier = new NetLegacyUserIdentifier() { Server = 1000, Usn = (long)user.ID },
                ShouldRestartAfter = Duration.FromTimeSpan(TimeSpan.FromSeconds(86400)),

                EncryptionToken = ByteString.CopyFromUtf8(encryptionToken)
            };

            user.ResetDataIfNeeded();
            
            await WriteDataAsync(response);
        }
    }
}
