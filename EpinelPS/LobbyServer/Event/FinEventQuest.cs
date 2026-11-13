using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/eventquest/fin")]
public class FinEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinEventQuest req = await ReadData<ReqFinEventQuest>();
        User user = GetUser();

        user.EventQuestData.TryAdd(req.EventQuestTid, false);

        JsonDb.Save();

        await WriteDataAsync(new ResFinEventQuest());
    }
}
