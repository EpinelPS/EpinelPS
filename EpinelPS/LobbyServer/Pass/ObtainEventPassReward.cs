using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/event/obtainreward")]
    public class ObtainEventPassReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "passId": 1037, "passRank": [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 ] }
            ReqObtainEventPassReward req = await ReadData<ReqObtainEventPassReward>(); //fields "PassId", "PassRank"
            User user = GetUser();

            ResObtainEventPassReward response = new(); // field Reward

            NetRewardData reward = new()
            {
                PassPoint = { }
            };

            PassHelper.ObtainPassRewards(user, ref reward, req.PassId, [.. req.PassRank]);
            response.Reward = reward;

            await WriteDataAsync(response);
        }
    }
}
