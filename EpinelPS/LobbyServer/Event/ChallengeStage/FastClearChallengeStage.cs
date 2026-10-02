using EpinelPS.Data;
using EpinelPS.Database;
using log4net;

namespace EpinelPS.LobbyServer.Event.ChallengeStage;

[GameRequest("/event/challengestage/fastclear")]
public class FastClearChallengeStage : LobbyMessage
{
    private static readonly ILog log = LogManager.GetLogger(typeof(FastClearChallengeStage));

    protected override async Task HandleAsync()
    {
        ReqFastClearChallengeEventStage req = await ReadData<ReqFastClearChallengeEventStage>();
        User user = GetUser();

        ResFastClearChallengeEventStage response = new();
        NetRewardData reward = new();
        NetRewardData dummyFirstClear = new();

        int clearCount = Math.Max(1, req.ClearCount);

        ChallengeStageHelper.ClearStage(user, req.StageId, ref reward, ref dummyFirstClear, isFirstClear: false, battleResult: 1, clearCount: clearCount);

        user.AddTrigger(Trigger.EventStageClear, clearCount, req.StageId);
        user.AddTrigger(Trigger.EventDungeonStageClear, clearCount, req.EventId);

        response.RemainTicket = ChallengeStageHelper.SubtractTicket(user, req.EventId, clearCount);
        response.Reward = reward;

        log.Info($"[ChallengeStage] User {user.ID} fast cleared challenge stage {req.StageId} x{clearCount} in event {req.EventId}. RemainTicket={response.RemainTicket}");

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
