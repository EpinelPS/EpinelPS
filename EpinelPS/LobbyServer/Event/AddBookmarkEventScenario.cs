using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/bookmark/event/scenario/add")]
public class AddBookmarkEventScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAddEventScenarioBookmark req = await ReadData<ReqAddEventScenarioBookmark>();
        User user = GetUser();

        if (!user.EventInfo.TryGetValue(req.EventId, out EventData? evt))
        {
            evt = new EventData();
            user.EventInfo[req.EventId] = evt;
        }

        if (!string.IsNullOrEmpty(req.ScenarioGroupId) && !evt.BookmarkedScenarios.Contains(req.ScenarioGroupId))
        {
            evt.BookmarkedScenarios.Add(req.ScenarioGroupId);
            JsonDb.Save();
        }

        ResAddEventScenarioBookmark response = new();
        await WriteDataAsync(response);
    }
}
