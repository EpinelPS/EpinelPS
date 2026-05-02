using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom;

[GameRequest("/simroom/popup-check/infinite")]
public class InfinitePopupCheck : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqInfinitePopupCheck>();
        var user = GetUser();

        ResInfinitePopupCheck response = new();

        user.ResetableData.SimRoomData.CurrentSeasonData.WasInfinitePopupChecked = true;
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}