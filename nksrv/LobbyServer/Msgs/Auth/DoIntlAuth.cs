using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Auth
{
    [PacketPath("/auth/intl")]
    public class DoIntlAuth : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqAuthIntl>();

            var response = new AuthIntlResponse();
            //response.BanInfo = new NetBanInfo() { BanId = 123, Description = "The server admin is sad today because the hinge on his HP laptop broke which happened to be an HP Elitebook 8470p, and the RAM controller exploded and then fixed itself, please contact him", StartAt = Timestamp.FromDateTime(DateTime.UtcNow), EndAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(256)) };
            response.AuthSuccess = new NetAuthSuccess() { AuthToken = req.Token, CentauriZoneId = "84", FirstAuth = "", PurchaseRestriction = new NetUserPurchaseRestriction() { PurchaseRestriction = PurchaseRestriction.Unknown2, UpdatedAt = 638546758794611090 } };
            WriteData(response);
        }
    }
}
