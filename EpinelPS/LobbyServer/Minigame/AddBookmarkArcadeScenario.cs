using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Minigame;

[GameRequest("/bookmark/arcade/scenario/add")]
public class AddBookmarkArcadeScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAddArcadeScenarioBookmark req = await ReadData<ReqAddArcadeScenarioBookmark>();
        User user = GetUser();

        if (!user.MiniGameScenarios.TryGetValue(req.ArcadeId, out MiniGameScenarios? ms))
        {
            ms = new MiniGameScenarios { ArcadeId = req.ArcadeId };
            user.MiniGameScenarios.Add(req.ArcadeId, ms);
        }

        if (!string.IsNullOrEmpty(req.ScenarioGroupId) && !ms.BookmarkedScenarios.Contains(req.ScenarioGroupId))
        {
            ms.BookmarkedScenarios.Add(req.ScenarioGroupId);
            JsonDb.Save();
        }

        ResAddArcadeScenarioBookmark response = new();
        await WriteDataAsync(response);
    }
}
