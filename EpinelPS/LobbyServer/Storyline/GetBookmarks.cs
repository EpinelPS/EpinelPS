using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Storyline;

[GameRequest("/storyline/bookmark/get")]
public class GetBookmarks : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetStorylineBookmarks req = await ReadData<ReqGetStorylineBookmarks>();

        ResGetStorylineBookmarks response = new();
        User user = GetUser();

        response.MainScenarioBookmarkList.AddRange(user.BookmarkedScenarios);

        if (req.EventIdList.Count > 0)
        {
            foreach (int eventId in req.EventIdList)
            {
                if (user.EventInfo.TryGetValue(eventId, out EventData? evt))
                {
                    response.EventScenarioBookmarkList.AddRange(evt.BookmarkedScenarios);
                }
            }
        }
        else
        {
            foreach (EventData evt in user.EventInfo.Values)
            {
                response.EventScenarioBookmarkList.AddRange(evt.BookmarkedScenarios);
            }
        }

        response.SideStoryBookmarkList.AddRange(user.SideStoryBookmarks);
        response.SubQuestBookmarkList.AddRange(user.SubQuestBookmarks);

        await WriteDataAsync(response);
    }
}
