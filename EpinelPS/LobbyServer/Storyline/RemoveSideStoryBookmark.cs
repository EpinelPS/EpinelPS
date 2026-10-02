using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Storyline;

[GameRequest("/bookmark/side-story/remove")]
public class RemoveSideStoryBookmark : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRemoveSideStoryBookmark req = await ReadData<ReqRemoveSideStoryBookmark>();
        User user = GetUser();

        if (user.SideStoryBookmarks.Remove(req.ScenarioGroupId))
        {
            JsonDb.Save();
        }

        ResRemoveSideStoryBookmark response = new();
        await WriteDataAsync(response);
    }
}
