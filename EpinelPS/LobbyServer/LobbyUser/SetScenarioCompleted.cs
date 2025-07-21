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
            ReqSetScenarioComplete req = await ReadData<ReqSetScenarioComplete>();
            User user = GetUser();

            ResSetScenarioComplete response = new()
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
