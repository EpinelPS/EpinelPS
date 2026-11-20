using EpinelPS.Data;
using EpinelPS.Database;
using log4net;

namespace EpinelPS.LobbyServer.Event.ChallengeStage;

[GameRequest("/event/challengestage/clear")]
public class ClearChallengeStage : LobbyMessage
{
    private static readonly ILog log = LogManager.GetLogger(typeof(ClearChallengeStage));

    protected override async Task HandleAsync()
    {
        ReqClearChallengeEventStage req = await ReadData<ReqClearChallengeEventStage>();
        User user = GetUser();

        ResClearChallengeEventStage response = new();

        int difficultId = 0;
        if (GameData.Instance.EventDungeonStageTable.TryGetValue(req.StageId, out var stageRec))
        {
            var diff = GameData.Instance.EventDungeonDifficultTable.Values.FirstOrDefault(d => d.StageGroup == stageRec.Group);
            if (diff != null) difficultId = diff.Id;
        }

        if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
        {
            eventData = new EventData
            {
                LastDay = user.GetDateDay(),
                FreeTicket = ChallengeStageHelper.GetTicket(user, req.EventId),
                Diff = difficultId
            };
            user.EventInfo.Add(req.EventId, eventData);
        }

        NetRewardData reward = new();
        NetRewardData firstClearReward = new();

        if (req.BattleResult == 1)
        {
            bool isFirstClear = !eventData.ClearedStages.Contains(req.StageId);
            if (isFirstClear)
            {
                eventData.ClearedStages.Add(req.StageId);
            }
            if (eventData.LastStage < req.StageId)
            {
                eventData.LastStage = req.StageId;
            }
            eventData.Diff = difficultId;

            ChallengeStageHelper.ClearStage(user, req.StageId, ref reward, ref firstClearReward, isFirstClear, req.BattleResult, 1);

            user.AddTrigger(Trigger.EventStageClear, 1, req.StageId);
            user.AddTrigger(Trigger.EventDungeonStageClear, 1, req.EventId);

            response.RemainTicket = ChallengeStageHelper.SubtractTicket(user, req.EventId, 1);
            response.Reward = reward;
            response.FirstClearReward = firstClearReward;

            log.Info($"[ChallengeStage] User {user.ID} cleared challenge stage {req.StageId} for event {req.EventId}. IsFirstClear={isFirstClear}, RemainTicket={response.RemainTicket}");
            JsonDb.Save();
        }
        else
        {
            response.RemainTicket = ChallengeStageHelper.GetTicket(user, req.EventId);
            response.Reward = reward;
            response.FirstClearReward = firstClearReward;
            log.Info($"[ChallengeStage] User {user.ID} battle failed (result={req.BattleResult}) for challenge stage {req.StageId} in event {req.EventId}");
        }

        await WriteDataAsync(response);
    }
}
