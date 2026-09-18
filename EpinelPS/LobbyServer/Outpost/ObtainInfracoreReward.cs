using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/infracore/reward")]
public class ObtainInfracoreReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainInfraCoreReward req = await ReadData<ReqObtainInfraCoreReward>();
        ResObtainInfraCoreReward response = new();

        User user = GetUser();

        int currentLevel = user.InfraCoreLvl;

        InfraCoreGradeRecord? gradeData = GameData.Instance.GetInfracoreGrade(currentLevel);
        if (gradeData != null && gradeData.RewardId > 0)
        {
            bool isReceived = user.InfraCoreRewardReceived.ContainsKey(currentLevel) && user.InfraCoreRewardReceived[currentLevel];

            if (!isReceived)
            {
                user.InfraCoreRewardReceived[currentLevel] = true;

                var reward = RewardUtils.RegisterRewardsForUser(user, gradeData.RewardId);
                response.Reward = reward;
                JsonDb.Save();
            }
        }

        await WriteDataAsync(response);
    }
}