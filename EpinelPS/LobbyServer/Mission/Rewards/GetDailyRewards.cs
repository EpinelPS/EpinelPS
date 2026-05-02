namespace EpinelPS.LobbyServer.Mission.Rewards;

[GameRequest("/mission/getrewarded/daily")]
public class GetDailyRewards : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetDailyRewardedData>();
        User user = GetUser();

        user.ResetDataIfNeeded();

        ResGetDailyRewardedData response = new();
        response.Ids.Add(user.ResetableData.CompletedDailyMissions);

        await WriteDataAsync(response);
    }
}
