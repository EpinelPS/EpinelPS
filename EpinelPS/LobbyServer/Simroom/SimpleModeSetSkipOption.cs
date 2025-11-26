using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/simplemode/setskipoption")]
    public class SimpleModeSetSkipOption : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetSimRoomSimpleModeSkipOption>();
            // ReqSetSimRoomSimpleModeSkipOption Fields
            // bool Enabled
            User user = GetUser();

            ResSetSimRoomSimpleModeSkipOption response = new();

            user.ResetableData.SimRoomData.IsSimpleModeSkipEnabled = req.Enabled;

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}