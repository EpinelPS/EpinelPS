using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/eventquest/list")]
public class ListEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListEventQuest req = await ReadData<ReqListEventQuest>();
        User user = GetUser();

        ResListEventQuest response = new();

        // quest ids are managerId * 100 + n (e.g. manager 10011 -> 1001101..1001108)
        foreach (KeyValuePair<int, bool> item in user.EventQuestData.Where(q => q.Key / 100 == req.EventQuestManagerTid))
        {
            response.EventQuests.Add(new NetEventQuestData()
            {
                EventQuestId = item.Key,
                IsReceived = item.Value
            });
        }

        await WriteDataAsync(response);
    }
}
