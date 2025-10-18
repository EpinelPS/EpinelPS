using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/obtainonereward")]
    public class ObtainOnePassReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "passId": 1037, "passRank": 1, "premiumReward": true }
            ReqObtainOnePassReward req = await ReadData<ReqObtainOnePassReward>(); //fields "PassId", "PassRank"
            User user = GetUser();

            ResObtainOnePassReward response = new(); // field Reward

            NetRewardData reward = new()
            {
                PassPoint = { }
            };

            PassHelper.ObtainOnePassRewards(user, ref reward, req.PassId, req.PassRank, req.PremiumReward);
            response.Reward = reward;

            await WriteDataAsync(response);
        }
    }
}
