using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/fastclearstage")]
    public class FastClearEventStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearEventStage req = await ReadData<ReqFastClearEventStage>();
            User user = GetUser();

            ResFastClearEventStage response = new();

            NetRewardData reward = new();
            NetRewardData bonusReward = new();
            ClearEventStageHelper.ClearStage(user, req.StageId, ref reward, ref bonusReward, 1, req.ClearCount); // always battleResult = 1 for fast clear

            user.AddTrigger(Trigger.EventDungeonStageClear, req.ClearCount, req.EventId);
            response.RemainTicket = EventStoryHelper.SubtractTicket(user, req.EventId, req.ClearCount);

            if (bonusReward.Item.Count > 0)
            {
                bonusReward.Item.ToList().ForEach(item =>
                {
                    user.AddTrigger(Trigger.ObtainEventCurrencyMaterial, item.Count, item.Tid);
                });
            }

            response.Reward = reward;
            response.BonusReward = bonusReward;

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}