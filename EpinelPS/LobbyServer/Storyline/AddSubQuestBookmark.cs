using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Storyline;

[GameRequest("/bookmark/sub-quest/add")]
public class AddSubQuestBookmark : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAddSubQuestBookmark req = await ReadData<ReqAddSubQuestBookmark>();
        User user = GetUser();

        if (!string.IsNullOrEmpty(req.ScenarioGroupId) && !user.SubQuestBookmarks.Contains(req.ScenarioGroupId))
        {
            user.SubQuestBookmarks.Add(req.ScenarioGroupId);
            JsonDb.Save();
        }

        ResAddSubQuestBookmark response = new();
        await WriteDataAsync(response);
    }
}
