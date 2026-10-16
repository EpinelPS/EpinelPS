using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/bookmark/scenario/add")]
public class AddBookmarkScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAddScenarioBookmark req = await ReadData<ReqAddScenarioBookmark>();
        User user = GetUser();

        if (!string.IsNullOrEmpty(req.ScenarioGroupId) && !user.BookmarkedScenarios.Contains(req.ScenarioGroupId))
        {
            user.BookmarkedScenarios.Add(req.ScenarioGroupId);
            JsonDb.Save();
        }

        ResAddScenarioBookmark response = new();
        await WriteDataAsync(response);
    }
}
