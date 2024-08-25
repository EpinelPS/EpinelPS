using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Msgs.Auth
{
    [PacketPath("/auth/intl")]
    public class DoIntlAuth : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqAuthIntl>();
            var response = new ResAuth();

            UsedAuthToken = req.Token;
            foreach (var item in JsonDb.Instance.LauncherAccessTokens)
            {
                if (item.Token == UsedAuthToken)
                {
                    UserId = item.UserID;
                }
            }
            if (UserId == 0)
            {
                response.AuthError = new NetAuthError() { ErrorCode = AuthErrorCode.AuthErrorCodeError };
            }
            else
            {
                var user = GetUser();

                if (user.IsBanned && user.BanEnd < DateTime.UtcNow)
                {
                    user.IsBanned = false;
                    user.BanId = 0;
                    user.BanStart = DateTime.MinValue;
                    user.BanEnd = DateTime.MinValue;
                    JsonDb.Save();
                }

                if (user.IsBanned)
                {
                    response.BanInfo = new NetBanInfo() { BanId = user.BanId, Description = "The server admin is sad today because the hinge on his HP laptop broke which happened to be an HP Elitebook 8470p, and the RAM controller exploded and then fixed itself, please contact him", StartAt = Timestamp.FromDateTime(DateTime.SpecifyKind(user.BanStart, DateTimeKind.Utc)), EndAt = Timestamp.FromDateTime(DateTime.SpecifyKind(user.BanEnd, DateTimeKind.Utc)) };
                }
                else
                {
                    response.AuthSuccess = new NetAuthSuccess() { AuthToken = req.Token, CentauriZoneId = "84", FirstAuth = false, PurchaseRestriction = new NetUserPurchaseRestriction() { PurchaseRestriction = PurchaseRestriction.PurchaseRestrictionChild, UpdatedAt = 638546758794611090 } };
                }
            }

            await WriteDataAsync(response);
        }
    }
}
