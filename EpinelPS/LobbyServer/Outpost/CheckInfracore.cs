using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/infracore/check")]
public class CheckInfracore : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCheckReceiveInfraCoreReward req = await ReadData<ReqCheckReceiveInfraCoreReward>();
        ResCheckReceiveInfraCoreReward response = new();

        User user = GetUser();

        bool isReceived = false;

        int currentLevel = user.InfraCoreLvl;

        InfraCoreGradeRecord? gradeData = GameData.Instance.GetInfracoreGrade(currentLevel);
        if (gradeData != null && gradeData.RewardId > 0)
        {
            isReceived = user.InfraCoreRewardReceived.ContainsKey(currentLevel) && user.InfraCoreRewardReceived[currentLevel];
        }

        response.IsReceived = isReceived;

        await WriteDataAsync(response);
    }
}
