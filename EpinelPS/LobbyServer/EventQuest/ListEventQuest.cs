using EpinelPS.Data;
using EpinelPS.Models;
using EpinelPS.LobbyServer.Archive;

namespace EpinelPS.LobbyServer.EventQuest;

[GameRequest("/eventquest/list")]
[GameRequest("/event-quest/list")]
[GameRequest("/archive/event-quest/list")]
[GameRequest("/event/quest/list")]
public class ListEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListEventQuest req = await ReadData<ReqListEventQuest>();
        User user = GetUser();
        ResListEventQuest response = new();

        int requestedManagerTid = req.EventQuestManagerTid;
        int managerId = 0;

        if (requestedManagerTid != 0)
        {
            managerId = ArchiveEventQuestHelper.ResolveManagerId(requestedManagerTid);
        }
        else if (user.ActivatedArchiveEventQuestId != 0)
        {
            managerId = ArchiveEventQuestHelper.ResolveManagerId(user.ActivatedArchiveEventQuestId);
        }

        user.ReceivedArchiveEventQuestRewardIds ??= [];

        if (managerId != 0)
        {
            var quests = GameData.Instance.GetArchiveEventQuestsForManager(managerId);
            foreach (var q in quests)
            {
                if (q.ConditionType == Category.End) continue;
                bool isReceived = user.ReceivedArchiveEventQuestRewardIds.Contains(q.Id);
                response.EventQuests.Add(new NetEventQuestData
                {
                    EventQuestId = q.Id,
                    IsReceived = isReceived
                });
            }
        }
        else
        {
            foreach (int qId in user.ClearedArchiveEventQuestIds)
            {
                bool isReceived = user.ReceivedArchiveEventQuestRewardIds.Contains(qId);
                response.EventQuests.Add(new NetEventQuestData
                {
                    EventQuestId = qId,
                    IsReceived = isReceived
                });
            }
        }

        await WriteDataAsync(response);
    }
}
