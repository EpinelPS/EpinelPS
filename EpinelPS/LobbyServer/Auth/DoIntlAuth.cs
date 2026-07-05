using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Auth;

[GameRequest("/auth/intl")]
public class DoIntlAuth : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAuthIntl req = await ReadData<ReqAuthIntl>();
        ResAuth response = new();
        var sdkUser = NetUtils.GetUser(req.Token, this.ctx).Item1;

        if (sdkUser == null)
        {
            response.AuthError = new NetAuthError() { ErrorCode = AuthErrorCode.Error };
        }
        else
        {
            UserId = sdkUser.ID;
            User user = GetUser();

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
                response.BanInfo = new NetBanInfo() { BanId = user.BanId, Description = "Unused", StartAt = Timestamp.FromDateTime(DateTime.SpecifyKind(user.BanStart, DateTimeKind.Utc)), EndAt = Timestamp.FromDateTime(DateTime.SpecifyKind(user.BanEnd, DateTimeKind.Utc)) };
            }
            else
            {
                response.AuthSuccess = new NetAuthSuccess() { AuthToken = req.Token, CentauriZoneId = "84", FirstAuth = false, PurchaseRestriction = new NetUserPurchaseRestriction() { PurchaseRestriction = PurchaseRestriction.Child, UpdatedAt = 638546758794611090 } };
            }
        }

        await WriteDataAsync(response);
    }
}
