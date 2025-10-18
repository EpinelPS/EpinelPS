using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/clearstage")]
    public class ClearEventStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearEventStage req = await ReadData<ReqClearEventStage>();
            User user = GetUser();

            ResClearEventStage response = new();

            int difficultId = 0;
            NetRewardData reward = new();
            NetRewardData bonusReward = new();
            ClearEventStageHelper.ClearStage(user, req.StageId, ref reward, ref bonusReward, req.BattleResult, 1); // always clearCount = 1 for normal clear

            if (user.EventInfo.TryGetValue(req.EventId, out EventData? eventData) && req.BattleResult == 1)
            {
                if (!eventData.ClearedStages.Contains(req.StageId))
                {
                    eventData.ClearedStages.Add(req.StageId);
                }
                eventData.LastStage = req.StageId;
                eventData.Diff = difficultId;
            }
            else
            {
                user.EventInfo.Add(req.EventId, new EventData() { LastStage = req.StageId, ClearedStages = [req.StageId] });
            }
            user.AddTrigger(Trigger.EventStageClear, 1, req.StageId);
            user.AddTrigger(Trigger.EventDungeonStageClear, 1, req.EventId);

            response.RemainTicket = 4;

            response.Reward = reward;
            response.BonusReward = bonusReward;

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}