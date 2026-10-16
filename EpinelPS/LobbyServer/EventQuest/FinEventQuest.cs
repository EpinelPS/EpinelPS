using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.LobbyServer.Archive;

namespace EpinelPS.LobbyServer.EventQuest;

[GameRequest("/eventquest/fin")]
[GameRequest("/event-quest/fin")]
[GameRequest("/event-quest/finish")]
public class FinEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinEventQuest req = await ReadData<ReqFinEventQuest>();
        User user = GetUser();
        ResFinEventQuest response = new();

        if (req.EventQuestTid != 0)
        {
            ArchiveEventQuestHelper.OnArchiveQuestCleared(user, req.EventQuestTid);
        }

        await WriteDataAsync(response);
    }
}
