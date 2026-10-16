using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Storyline;

[GameRequest("/bookmark/side-story/add")]
public class AddSideStoryBookmark : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAddSideStoryBookmark req = await ReadData<ReqAddSideStoryBookmark>();
        User user = GetUser();

        if (!string.IsNullOrEmpty(req.ScenarioGroupId) && !user.SideStoryBookmarks.Contains(req.ScenarioGroupId))
        {
            user.SideStoryBookmarks.Add(req.ScenarioGroupId);
            JsonDb.Save();
        }

        ResAddSideStoryBookmark response = new();
        await WriteDataAsync(response);
    }
}
