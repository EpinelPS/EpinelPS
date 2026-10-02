using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/event-quest/clear")]
public class ClearArchiveEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArchiveEventQuest req = await ReadData<ReqClearArchiveEventQuest>();
        User user = GetUser();
        ResClearArchiveEventQuest response = new();

        if (req.ArchiveEventQuestId != 0)
        {
            ArchiveEventQuestHelper.OnArchiveQuestCleared(user, req.ArchiveEventQuestId);
        }

        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/event-quest/reset")]
public class ResetArchiveEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqResetArchiveEventQuest req = await ReadData<ReqResetArchiveEventQuest>();
        User user = GetUser();
        ResResetArchiveEventQuest response = new();

        var arm = GameData.Instance.archiveRecordManagerTable.GetValueOrDefault(req.ArchiveRecordManagerId);
        if (arm != null)
        {
            int managerId = ArchiveEventQuestHelper.ResolveManagerId(req.ArchiveRecordManagerId);

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

                user.ClearedArchiveEventQuestIds.RemoveAll(id => quests.Contains(id));
                user.ClearedArchiveEventQuestStageIds.RemoveAll(id => stages.Contains(id));
                user.ReceivedArchiveEventQuestRewardIds.RemoveAll(id => quests.Contains(id));
                user.ClaimedArchiveEventClearRewardIds.Remove(req.ArchiveRecordManagerId);
                if (user.ActivatedArchiveEventQuestId == req.ArchiveRecordManagerId)
                {
                    user.ActivatedArchiveEventQuestId = 0;
                }
                JsonDb.Save();
            }
        }

        await WriteDataAsync(response);
    }
}

 [GameRequest("/archive/event-quest/reward/acquire")]
 [GameRequest("/archive/event-quest/acquire-reward")]
 [GameRequest("/archive/event-quest/reward/obtain")]
 [GameRequest("/archive/event-quest/obtain-reward")]
 [GameRequest("/archive/event-quest/obtain")]
 public class AcquireArchiveEventQuestReward : LobbyMessage
 {
     protected override async Task HandleAsync()
     {
         ReqAcquireArchiveEventQuestReward req = await ReadData<ReqAcquireArchiveEventQuestReward>();
         User user = GetUser();
         ResAcquireArchiveEventQuestReward response = new()
         {
             Reward = new NetRewardData()
         };
 
         user.ClaimedArchiveEventClearRewardIds ??= [];
 
         var arm = GameData.Instance.archiveRecordManagerTable.GetValueOrDefault(req.ArchiveRecordManagerId);
         if (arm != null && arm.EventQuestClearRewardId > 0)
         {
             if (!user.ClaimedArchiveEventClearRewardIds.Contains(req.ArchiveRecordManagerId))
             {
                 user.ClaimedArchiveEventClearRewardIds.Add(req.ArchiveRecordManagerId);
                 response.Reward = RewardUtils.RegisterRewardsForUser(user, arm.EventQuestClearRewardId);
                 JsonDb.Save();
             }
         }

        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/event-quest/stage/enter")]
[GameRequest("/archive/event-quest/enterstage")]
public class EnterArchiveEventQuestStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqEnterArchiveEventQuestStage>();
        ResEnterArchiveEventQuestStage response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/event-quest/stage/clear")]
[GameRequest("/archive/event-quest/clearstage")]
public class ClearArchiveEventQuestStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArchiveEventQuestStage req = await ReadData<ReqClearArchiveEventQuestStage>();
        User user = GetUser();
        ResClearArchiveEventQuestStage response = new();

        if (req.EventQuestStageId != 0 && req.BattleResult == 1)
        {
            ArchiveEventQuestHelper.OnStageCleared(user, req.EventQuestStageId);
        }

        await WriteDataAsync(response);
    }
}
