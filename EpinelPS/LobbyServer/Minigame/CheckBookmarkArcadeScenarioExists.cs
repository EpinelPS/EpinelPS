using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Minigame;

[GameRequest("/bookmark/arcade/scenario/exist")]
public class CheckBookmarkArcadeScenarioExists : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqExistArcadeScenarioBookmark req = await ReadData<ReqExistArcadeScenarioBookmark>();
        User user = GetUser();

        ResExistArcadeScenarioBookmark response = new();

        if (user.MiniGameScenarios.TryGetValue(req.ArcadeId, out MiniGameScenarios? ms))
        {
            if (req.BookmarkList.Count > 0)
            {
                foreach (string item in req.BookmarkList)
                {
                    if (ms.BookmarkedScenarios.Contains(item))
                    {
                        response.ExistBookmarkList.Add(item);
                    }
                }
            }
            else
            {
                response.ExistBookmarkList.AddRange(ms.BookmarkedScenarios);
            }
        }

        await WriteDataAsync(response);
    }
}
