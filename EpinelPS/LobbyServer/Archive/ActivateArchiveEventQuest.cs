using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/event-quest/activate")]
[GameRequest("/event-quest/activate")]
[GameRequest("/event/event-quest/activate")]
public class ActivateArchiveEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqActivateArchiveEventQuest req = await ReadData<ReqActivateArchiveEventQuest>();
        User user = GetUser();
        ResActivateArchiveEventQuest response = new();

        ArchiveRecordManagerRecord record = GameData.Instance.archiveRecordManagerTable.GetValueOrDefault(req.ArchiveRecordManagerId)
            ?? throw new BadHttpRequestException($"Unknown archive event quest record {req.ArchiveRecordManagerId}", 400);

        if (GameConfig.Root.ArchiveUnlockAll != true && !user.UnlockedArchiveEventQuestIds.Contains(record.Id))
        {
            throw new BadHttpRequestException($"Archive event quest record {req.ArchiveRecordManagerId} is not unlocked", 400);
        }

        int managerId = ArchiveEventQuestHelper.ResolveManagerId(record.Id);
        if (managerId != 0)
        {
            var quests = GameData.Instance.archiveEventQuestRecords.Values
                .Where(q => q.EventQuestManagerId == managerId)
                .Select(q => q.Id)
                .ToHashSet();
            var stages = GameData.Instance.eventQuestStageRecords.Values
                .Where(s => s.ArchiveEventQuestManagerId == managerId)
                .Select(s => s.Id)
                .ToHashSet();

            // When activating an event quest:
            // If starting a different event quest, or if this event quest was previously completed,
            // start fresh at 0% with Quest 1.
            // If already in-progress on this event, preserve player's progress across logins and scene switches!
            bool isCompleted = ArchiveEventQuestHelper.GetCurrentActiveQuest(user, managerId) == null &&
                               user.ClearedArchiveEventQuestIds.Any(id => quests.Contains(id));
            if (user.ActivatedArchiveEventQuestId != record.Id || isCompleted)
            {
                user.ClearedArchiveEventQuestIds.RemoveAll(id => quests.Contains(id));
                user.ClearedArchiveEventQuestStageIds.RemoveAll(id => stages.Contains(id));
                user.ReceivedArchiveEventQuestRewardIds.RemoveAll(id => quests.Contains(id));
                user.ClaimedArchiveEventClearRewardIds.Remove(record.Id);
            }
        }

        user.ActivatedArchiveEventQuestId = record.Id;
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
