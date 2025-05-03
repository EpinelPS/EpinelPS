using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/SetScenarioComplete")]
    public class SetScenarioCompleted : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetScenarioComplete>();
            var user = GetUser();

            var response = new ResSetScenarioComplete
            {
                Reward = new NetRewardData()
            };

            user.CompletedScenarios.Add(req.ScenarioId);

            if (GameData.Instance.ScenarioRewards.TryGetValue(req.ScenarioId, out ScenarioRewardRecord? record))
            {
                response.Reward = RewardUtils.RegisterRewardsForUser(user, record.reward_id);
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
