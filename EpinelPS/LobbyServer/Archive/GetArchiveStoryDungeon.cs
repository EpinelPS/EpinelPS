using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/storydungeon/get")]
public class GetArchiveStoryDungeon : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArchiveStoryDungeon req = await ReadData<ReqGetArchiveStoryDungeon>(); // has EventId field
        int evid = req.EventId;
        User user = GetUser();

        // Ensure the EventInfo dictionary contains the requested EventId
        if (!user.EventInfo.TryGetValue(evid, out EventData? eventData))
        {
            eventData = new EventData
            {
                CompletedScenarios = [],
                Diff = 0,
                LastStage = 0
            };
            user.EventInfo[evid] = eventData;
            JsonDb.Save();
        }

        // Prepare the response
        ResGetArchiveStoryDungeon response = new()
        {
            TeamData = new NetUserTeamData
            {
                LastContentsTeamNumber = 1,
                Type = 1
            }
        };

        // Resolve all difficulties associated with this event
        List<ArchiveEventDungeonDifficultRecord> difficulties = [];
        var story = GameData.Instance.archiveEventStoryRecords.Values
            .FirstOrDefault(s => s.EventId == evid || s.Id == evid);

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
                    difficulties.AddRange(diffs);
                }
            }
        }

        if (difficulties.Count > 0)
        {
            // Order by Order (Normal first, then Hard)
            foreach (var diff in difficulties.OrderBy(d => d.Order).ThenBy(d => d.Id))
            {
                var stagesInGroup = GameData.Instance.archiveEventDungeonStageRecords.Values
                    .Where(s => s.Group == diff.StageGroup)
                    .OrderBy(s => s.Step)
                    .ToList();

                int lastClearedStageId = 0;
                var clearedInGroup = stagesInGroup
                    .Where(s => eventData.ClearedStages.Contains(s.Id))
                    .ToList();

                if (clearedInGroup.Count > 0)
                {
                    lastClearedStageId = clearedInGroup.OrderByDescending(s => s.Step).First().Id;
                }
                else if (eventData.LastStage > 0 && stagesInGroup.Any(s => s.Id == eventData.LastStage))
                {
                    lastClearedStageId = eventData.LastStage;
                }

                response.LastClearedArchiveStageList.Add(new NetLastClearedArchiveStage
                {
                    DifficultyId = diff.Id,
                    StageId = lastClearedStageId
                });
            }
        }
        else
        {
            // Fallback for events with no mapped difficulties
            response.LastClearedArchiveStageList.Add(new NetLastClearedArchiveStage
            {
                DifficultyId = eventData.Diff,
                StageId = eventData.LastStage
            });
        }

        // Send the response
        await WriteDataAsync(response);
    }
}
