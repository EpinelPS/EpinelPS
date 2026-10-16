using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Archive.Minigame.TowerDefense;

[GameRequest("/archive/minigame/towerdefense/finish")]
public class FinishArchiveTowerDefense : LobbyMessage
{
    private static readonly ILog log = LogManager.GetLogger(typeof(FinishArchiveTowerDefense));

    protected override async Task HandleAsync()
    {
        ReqFinishArchiveTowerDefense req = await ReadData<ReqFinishArchiveTowerDefense>();
        User user = GetUser();

        // 1. Resolve candidate stage groups for this event to disambiguate the stage record
        HashSet<int> stageGroups = [];
        var story = GameData.Instance.archiveEventStoryRecords.Values
            .FirstOrDefault(s => s.EventId == req.EventId || s.Id == req.EventId);

        if (story != null)
        {
            List<int> dungeonIds = [story.DungeonId1];
            if (story.DungeonId2 > 0) dungeonIds.Add(story.DungeonId2);

            foreach (int dId in dungeonIds)
            {
                if (GameData.Instance.ArchiveEventDungeonreRecordRaws.TryGetValue(dId, out var dungeon))
                {
                    var diffs = GameData.Instance.ArchiveEventDungeonDifficultRecords.Values
                        .Where(d => d.Group == dungeon.DifficultGroup);
                    foreach (var diff in diffs)
                    {
                        stageGroups.Add(diff.StageGroup);
                    }
                }
            }
        }

        // 2. Find matching ArchiveEventDungeonStageRecord (e.g. Id=104510106 for EX-1 with StageId=100101)
        ArchiveEventDungeonStageRecord? stageRecord = null;
        if (stageGroups.Count > 0)
        {
            stageRecord = GameData.Instance.archiveEventDungeonStageRecords.Values.FirstOrDefault(s =>
                stageGroups.Contains(s.Group) &&
                (s.StageId == req.StageId || s.Id == req.StageId) &&
                s.StageContentsType == EventDungeonContentsType.TowerDefense);
        }

        if (stageRecord == null)
        {
            // Global fallback
            stageRecord = GameData.Instance.archiveEventDungeonStageRecords.Values.FirstOrDefault(s =>
                (s.StageId == req.StageId || s.Id == req.StageId) &&
                s.StageContentsType == EventDungeonContentsType.TowerDefense);
        }

        int clearedStageTableId = stageRecord?.Id ?? req.StageId;

        // 3. Update User EventInfo upon victory
        if (!user.EventInfo.TryGetValue(req.EventId, out var eventData))
        {
            eventData = new EventData();
            user.EventInfo[req.EventId] = eventData;
        }

        if (req.IsWin)
        {
            if (!eventData.ClearedStages.Contains(clearedStageTableId))
            {
                eventData.ClearedStages.Add(clearedStageTableId);
            }

            eventData.LastStage = clearedStageTableId;

            if (stageRecord != null)
            {
                var diff = GameData.Instance.ArchiveEventDungeonDifficultRecords.Values
                    .FirstOrDefault(d => d.StageGroup == stageRecord.Group);
                if (diff != null)
                {
                    eventData.Diff = diff.Id;
                }
            }

            // Track minigame progress in user.TowerDefenseDatas
            if (!user.TowerDefenseDatas.TryGetValue(req.EventId, out var tdData))
            {
                tdData = new TowerDefenseData();
                user.TowerDefenseDatas[req.EventId] = tdData;
            }

            if (!tdData.ClearedStageIdList.Contains(req.StageId))
            {
                tdData.ClearedStageIdList.Add(req.StageId);

                // Check for first clear rewards from EventTowerDefenseStageTable
                if (GameData.Instance.EventTowerDefenseStageTable.TryGetValue(req.StageId, out var tdStageRec))
                {
                    if (tdStageRec != null && tdStageRec.StageFirstClearReward > 0)
                    {
                        RewardUtils.RegisterRewardsForUser(user, tdStageRec.StageFirstClearReward);
                    }
                }
            }

            if (req.Score > tdData.ChallengeMaxScore)
            {
                tdData.ChallengeMaxScore = req.Score;
            }

            log.Info($"[ArchiveTowerDefense] Cleared Stage {clearedStageTableId} (MinigameStageId={req.StageId}) for Event {req.EventId}");
            JsonDb.Save();
        }

        ResFinishArchiveTowerDefense response = new();
        await WriteDataAsync(response);
    }
}
