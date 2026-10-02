using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;
using EpinelPS.LobbyServer.Archive;

namespace EpinelPS.LobbyServer.EventQuest;

[GameRequest("/eventquest/stage/clear")]
[GameRequest("/eventquest/clearstage")]
[GameRequest("/event-quest/clearstage")]
[GameRequest("/event-quest/stage/clear")]
[GameRequest("/event/event-quest/clearstage")]
public class ClearEventQuestStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearEventQuestStage req = await ReadData<ReqClearEventQuestStage>();
        User user = GetUser();
        ResClearEventQuestStage response = new()
        {
            Reward = new NetRewardData(),
            UserLevelUpReward = new NetRewardData()
        };

        if (req.StageId != 0 && req.BattleResult == 1)
        {
            if (GameData.Instance.eventQuestStageRecords.TryGetValue(req.StageId, out var stageRec) &&
                stageRec.RewardId > 0 &&
                !user.ClearedArchiveEventQuestStageIds.Contains(req.StageId))
            {
                response.Reward = RewardUtils.RegisterRewardsForUser(user, stageRec.RewardId);
            }

            ArchiveEventQuestHelper.OnStageCleared(user, req.StageId);

            if (req.EventQuestId != 0 &&
                GameData.Instance.archiveEventQuestRecords.TryGetValue(req.EventQuestId, out var questRec))
            {
                if (questRec.ConditionType == Category.EventQuestStageClear && questRec.ConditionValue == req.StageId)
                {
                    ArchiveEventQuestHelper.OnArchiveQuestCleared(user, req.EventQuestId);
                }
                else if (questRec.ConditionType == Category.EventQuestStageGroupClear)
                {
                    var groupStages = GameData.Instance.eventQuestStageRecords.Values
                        .Where(s => s.GroupId == questRec.ConditionValue)
                        .ToList();

                    if (groupStages.Count > 0 && groupStages.All(s => user.ClearedArchiveEventQuestStageIds.Contains(s.Id)))
                    {
                        ArchiveEventQuestHelper.OnArchiveQuestCleared(user, req.EventQuestId);
                    }
                }
            }
        }

        await WriteDataAsync(response);
    }
}
