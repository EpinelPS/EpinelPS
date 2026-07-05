using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/User/SetScenarioComplete")]
public class SetScenarioCompleted : LobbyMessage
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

        if (GameData.Instance.ScenarioRewards.TryGetValue(req.ScenarioId, out ScenarioRewardsRecord? record))
        {
            response.Reward = RewardUtils.RegisterRewardsForUser(user, record.RewardId);
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
