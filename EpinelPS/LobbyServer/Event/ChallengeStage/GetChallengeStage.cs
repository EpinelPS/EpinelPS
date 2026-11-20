using EpinelPS.Data;
using EpinelPS.Database;
using log4net;

namespace EpinelPS.LobbyServer.Event.ChallengeStage;

[GameRequest("/event/challengestage/get")]
public class GetChallengeStage : LobbyMessage
{
    private static readonly ILog log = LogManager.GetLogger(typeof(GetChallengeStage));

    protected override async Task HandleAsync()
    {
        ReqChallengeEventStageData req = await ReadData<ReqChallengeEventStageData>();
        User user = GetUser();

        if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
        {
            eventData = new()
            {
                LastStage = 0,
                LastDay = user.GetDateDay(),
                FreeTicket = ChallengeStageHelper.GetChallengeStageCount(req.EventId)
            };
            user.EventInfo.Add(req.EventId, eventData);
        }

        ResChallengeEventStageData response = new()
        {
            RemainTicket = ChallengeStageHelper.GetTicket(user, req.EventId),
            TeamData = new NetUserTeamData
            {
                Type = (int)TeamType.StoryEvent
            },
        };

        if (user.UserTeams.TryGetValue((int)TeamType.StoryEvent, out NetUserTeamData? teamData))
        {
            response.TeamData = teamData;
        }

        if (eventData.ClearedStages.Count > 0)
        {
            foreach (var stageId in eventData.ClearedStages)
            {
                int diffId = eventData.Diff;
                if (GameData.Instance.EventDungeonStageTable.TryGetValue(stageId, out var stageRec))
                {
                    var diff = GameData.Instance.EventDungeonDifficultTable.Values.FirstOrDefault(d => d.StageGroup == stageRec.Group);
                    if (diff != null) diffId = diff.Id;
                }
                response.LastClearedEventStageList.Add(new NetLastClearedEventStageData
                {
                    DifficultyId = diffId,
                    StageId = stageId
                });
            }
        }
        else
        {
            response.LastClearedEventStageList.Add(new NetLastClearedEventStageData
            {
                DifficultyId = eventData.Diff,
                StageId = eventData.LastStage
            });
        }

        log.Debug($"[ChallengeStage] GetChallengeStage EventId={req.EventId}, RemainTicket={response.RemainTicket}, ClearedStagesCount={eventData.ClearedStages.Count}");

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
