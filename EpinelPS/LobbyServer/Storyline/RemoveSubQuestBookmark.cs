using EpinelPS.Database;
using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Storyline;

[GameRequest("/bookmark/sub-quest/remove")]
public class RemoveSubQuestBookmark : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRemoveSubQuestBookmark req = await ReadData<ReqRemoveSubQuestBookmark>();
        User user = GetUser();

        if (user.SubQuestBookmarks.Remove(req.ScenarioGroupId))
        {
            JsonDb.Save();
        }

        ResRemoveSubQuestBookmark response = new();
        await WriteDataAsync(response);
    }
}
