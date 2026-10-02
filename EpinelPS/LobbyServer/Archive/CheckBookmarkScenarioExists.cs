using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/bookmark/scenario/exist")]
public class CheckBookmarkScenarioExists : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqExistScenarioBookmark req = await ReadData<ReqExistScenarioBookmark>();
        User user = GetUser();

        ResExistScenarioBookmark response = new();

        if (req.BookmarkList.Count > 0)
        {
            foreach (string item in req.BookmarkList)
            {
                if (user.BookmarkedScenarios.Contains(item))
                {
                    response.ExistBookmarkList.Add(item);
                }
            }
        }
        else
        {
            response.ExistBookmarkList.AddRange(user.BookmarkedScenarios);
        }

        await WriteDataAsync(response);
    }
}
