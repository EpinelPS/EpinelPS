using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.StoryEvent;

[GameRequest("/event/storydungeon/get")]
public class GetStoryDungeon : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqStoryDungeonEventData req = await ReadData<ReqStoryDungeonEventData>();
        User user = GetUser();

        // Get user event data, if not exist, create new one
        if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
        {
            eventData = new() { LastDay = user.GetDateDay(), FreeTicket = 5 };
            user.EventInfo.TryAdd(req.EventId, eventData);
        }


        ResStoryDungeonEventData response = new()
        {
            RemainTicket = EventStoryHelper.GetTicket(user, req.EventId),
            TeamData = new NetUserTeamData
            {
                Type = (int)TeamType.StoryEvent
            },
        };

        if (user.UserTeams.TryGetValue((int)TeamType.StoryEvent, out NetUserTeamData? teamData))
        {
            response.TeamData = teamData;
        }
        // Resolve all difficulties associated with this event
        List<EventDungeonDifficultRecord> difficulties = [];
        var story = GameData.Instance.EventStoryTable.Values
            .FirstOrDefault(s => s.EventId == req.EventId || s.Id == req.EventId);

        if (story != null)
        {
            List<int> dungeonIds = [story.DungeonId1];
            if (story.DungeonId2 > 0) dungeonIds.Add(story.DungeonId2);

            foreach (int dId in dungeonIds)
            {
                if (GameData.Instance.EventDungeonTable.TryGetValue(dId, out var dungeon))
                {
                    var diffs = GameData.Instance.EventDungeonDifficultTable.Values
                        .Where(d => d.Group == dungeon.DifficultGroup);
                    difficulties.AddRange(diffs);
                }
            }
        }

        if (difficulties.Count > 0)
        {
            foreach (var diff in difficulties.OrderBy(d => d.Order).ThenBy(d => d.Id))
            {
                var stagesInGroup = GameData.Instance.EventDungeonStageTable.Values
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

                response.LastClearedEventStageList.Add(new NetLastClearedEventStageData
                {
                    DifficultyId = diff.Id,
                    StageId = lastClearedStageId
                });
            }
        }
        else
        {
            // Fallback for events with no mapped difficulties
            response.LastClearedEventStageList.Add(new NetLastClearedEventStageData
            {
                DifficultyId = eventData.Diff,
                StageId = eventData.LastStage
            });
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
