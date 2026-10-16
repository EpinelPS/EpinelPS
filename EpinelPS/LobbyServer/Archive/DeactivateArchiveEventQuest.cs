using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/event-quest/deactivate")]
[GameRequest("/event-quest/deactivate")]
[GameRequest("/event/event-quest/deactivate")]
public class DeactivateArchiveEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqDeactivateArchiveEventQuest>();
        User user = GetUser();
        ResDeactivateArchiveEventQuest response = new();

        user.ActivatedArchiveEventQuestId = 0;
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
