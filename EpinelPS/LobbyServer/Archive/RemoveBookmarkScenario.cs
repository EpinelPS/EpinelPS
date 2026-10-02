using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/bookmark/scenario/remove")]
public class RemoveBookmarkScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRemoveScenarioBookmark req = await ReadData<ReqRemoveScenarioBookmark>();
        User user = GetUser();

        if (user.BookmarkedScenarios.Remove(req.ScenarioGroupId))
        {
            JsonDb.Save();
        }

        ResRemoveScenarioBookmark response = new();
        await WriteDataAsync(response);
    }
}
