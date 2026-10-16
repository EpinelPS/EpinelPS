using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/scenario/watchalbumlog")]
public class WatchAlbumScenarioLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqWatchAlbumScenarioLog req = await ReadData<ReqWatchAlbumScenarioLog>();
        Logging.WriteLine($"[WatchAlbumScenarioLog] User watched album resource {req.AlbumResourceId} (type: {req.ScenarioDataType})", LogType.Info);
        User user = GetUser();
        if (GameData.Instance.albumResourceRecords.TryGetValue(req.AlbumResourceId, out var albumRec) && !string.IsNullOrEmpty(albumRec.ScenarioGroupId))
        {
            var matchingQuest = GameData.Instance.archiveEventQuestRecords.Values
                .FirstOrDefault(q => q.EndScenarioId == albumRec.ScenarioGroupId);
            if (matchingQuest != null)
            {
                Archive.ArchiveEventQuestHelper.OnArchiveQuestCleared(user, matchingQuest.Id);
            }
        }

        ResWatchAlbumScenarioLog response = new();
        await WriteDataAsync(response);
    }
}
