namespace EpinelPS.LobbyServer.Event.Minigame.AZX;

[GameRequest("/event/minigame/azx/acquire/achievementmission/reward")]
public class AcquireAzxAchievementMissionReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // ReqAcquireMiniGameAzxAchievementMissionReward Fields
        //  int AzxId
        //  RepeatedField<int> AchievementMissionIdList
        ReqAcquireMiniGameAzxAchievementMissionReward req = await ReadData<ReqAcquireMiniGameAzxAchievementMissionReward>();
        User user = GetUser();

        // ResAcquireMiniGameAzxAchievementMissionReward Fields
        //  NetRewardData Reward
        ResAcquireMiniGameAzxAchievementMissionReward response = new();

        NetRewardData reward = new();
        AzxHelper.AcquireReward(user, ref reward, req.AzxId, req.AchievementMissionIdList);

        response.Reward = reward;


        await WriteDataAsync(response);
    }
}