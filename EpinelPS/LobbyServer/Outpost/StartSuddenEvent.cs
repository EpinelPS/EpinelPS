using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/suddenevent/start")]
    public class StartSuddenEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqStartSuddenEvent req = await ReadData<ReqStartSuddenEvent>();
            User user = GetUser();
            ResStartSuddenEvent response = new()
            {
                // Todo: fill in currency data
                Currency = new NetUserCurrencyData()
                {
                }
            };

            // mark the scenario as completed
            if (req.Tid == 0) throw new Exception("tid is 0");
            if (GameData.Instance.OutpostConditionTriggerTable.TryGetValue(req.Tid, out OutpostConditionTrigger? trigger))
            {
                response.Reward = RewardUtils.RegisterRewardsForUser(user, trigger.reward_id);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}