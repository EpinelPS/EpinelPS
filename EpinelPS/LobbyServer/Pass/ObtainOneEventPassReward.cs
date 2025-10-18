using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/event/obtainonereward")]
    public class ObtainOneEventPassReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "passId": 1037, "passRank": 1, "premiumReward": true }
            ReqObtainOneEventPassReward req = await ReadData<ReqObtainOneEventPassReward>(); //fields "PassId", "PassRank"
            User user = GetUser();

            ResObtainOneEventPassReward response = new(); // field Reward

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
