using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/scenario/watchalbumlog")]
public class WatchAlbumScenarioLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqWatchAlbumScenarioLog req = await ReadData<ReqWatchAlbumScenarioLog>();
        Logging.WriteLine($"[WatchAlbumScenarioLog] User watched album resource {req.AlbumResourceId} (type: {req.ScenarioDataType})", LogType.Info);

        ResWatchAlbumScenarioLog response = new();
        await WriteDataAsync(response);
    }
}
