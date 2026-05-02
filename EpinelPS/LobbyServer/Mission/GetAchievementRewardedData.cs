namespace EpinelPS.LobbyServer.Mission;

[GameRequest("/mission/getrewarded/achievement")]
public class GetAchievementRewardedData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetAchievementRewardedData>();
        User user = GetUser();

        ResGetAchievementRewardedData response = new();
        response.Ids.AddRange(user.CompletedAchievements);

        await WriteDataAsync(response);
    }
}
