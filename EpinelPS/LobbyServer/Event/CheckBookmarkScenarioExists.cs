using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/bookmark/event/scenario/exist")]
public class CheckBookmarkScenarioExists : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqExistEventScenarioBookmark req = await ReadData<ReqExistEventScenarioBookmark>();
        User user = GetUser();

        ResExistEventScenarioBookmark response = new();

        if (user.EventInfo.TryGetValue(req.EventId, out EventData? evt))
        {
            if (req.BookmarkList.Count > 0)
            {
                foreach (string item in req.BookmarkList)
                {
                    if (evt.BookmarkedScenarios.Contains(item))
                    {
                        response.ExistBookmarkList.Add(item);
                    }
                }
            }
            else
            {
                response.ExistBookmarkList.AddRange(evt.BookmarkedScenarios);
            }
        }

        await WriteDataAsync(response);
    }
}
