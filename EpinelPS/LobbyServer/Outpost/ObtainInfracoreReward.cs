using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/infracore/reward")]
    public class ObtainInfracoreReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainInfraCoreReward req = await ReadData<ReqObtainInfraCoreReward>();
            ResObtainInfraCoreReward response = new();

            User user = GetUser();

            int currentLevel = user.InfraCoreLvl;

             Dictionary<int, InfracoreRecord> gradeTable = GameData.Instance.InfracoreTable;
            if (gradeTable.TryGetValue(currentLevel, out var gradeData))
            {
                if (gradeData.reward_id > 0)
                {
                    bool isReceived = user.InfraCoreRewardReceived.ContainsKey(currentLevel) && user.InfraCoreRewardReceived[currentLevel];
                    
                    if (!isReceived)
                    {
                        user.InfraCoreRewardReceived[currentLevel] = true;
                        
                        var reward = RewardUtils.RegisterRewardsForUser(user, gradeData.reward_id);
                        response.Reward = reward;
                    }
                }
            }

            await WriteDataAsync(response);
        }
    }
}