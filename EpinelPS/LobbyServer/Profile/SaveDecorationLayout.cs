using EpinelPS.Utils;
using EpinelPS.Database;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Profile
{
    [PacketPath("/ProfileCard/DecorationLayout/Save")]
    public class SaveDecorationLayout : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSaveProfileCardDecorationLayout req = await ReadData<ReqSaveProfileCardDecorationLayout>();
            User user = GetUser();
            ResSaveProfileCardDecorationLayout response = new();

            user.ProfileCardDecoration.Layout = req.Layout;
            JsonDb.Save();
            // response.EditingBannedUntil = DateTime.UtcNow.ToTimestamp();
            await WriteDataAsync(response);
        }
    }
}