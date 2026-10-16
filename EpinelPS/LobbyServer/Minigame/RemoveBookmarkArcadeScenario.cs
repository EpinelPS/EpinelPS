using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Minigame;

[GameRequest("/bookmark/arcade/scenario/remove")]
public class RemoveBookmarkArcadeScenario : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRemoveArcadeScenarioBookmark req = await ReadData<ReqRemoveArcadeScenarioBookmark>();
        User user = GetUser();

        if (user.MiniGameScenarios.TryGetValue(req.ArcadeId, out MiniGameScenarios? ms))
        {
            if (ms.BookmarkedScenarios.Remove(req.ScenarioGroupId))
            {
                JsonDb.Save();
            }
        }

        ResRemoveArcadeScenarioBookmark response = new();
        await WriteDataAsync(response);
    }
}
