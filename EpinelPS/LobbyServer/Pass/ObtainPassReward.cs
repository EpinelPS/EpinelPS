namespace EpinelPS.LobbyServer.Pass;

[GameRequest("/pass/obtainreward")]
public class ObtainPassReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // { "passId": 1037, "passRank": [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 ] }
        ReqObtainPassReward req = await ReadData<ReqObtainPassReward>(); //fields "PassId", "PassRank"
        User user = GetUser();

        ResObtainPassReward response = new(); // field Reward

        NetRewardData reward = new()
        {
            PassPoint = { }
        };

        PassHelper.ObtainPassRewards(user, ref reward, req.PassId, [.. req.PassRank]);
        response.Reward = reward;

        await WriteDataAsync(response);
    }
}
