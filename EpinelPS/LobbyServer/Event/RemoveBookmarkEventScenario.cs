using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/bookmark/event/scenario/remove")]
public class RemoveBookmarkEventScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRemoveEventScenarioBookmark req = await ReadData<ReqRemoveEventScenarioBookmark>();
        User user = GetUser();

        if (user.EventInfo.TryGetValue(req.EventId, out EventData? evt))
        {
            if (evt.BookmarkedScenarios.Remove(req.ScenarioGroupId))
            {
                JsonDb.Save();
            }
        }

        ResRemoveEventScenarioBookmark response = new();
        await WriteDataAsync(response);
    }
}
