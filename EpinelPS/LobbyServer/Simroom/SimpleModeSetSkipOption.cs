using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom;

[GameRequest("/simroom/simplemode/setskipoption")]
public class SimpleModeSetSkipOption : LobbyMessage
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