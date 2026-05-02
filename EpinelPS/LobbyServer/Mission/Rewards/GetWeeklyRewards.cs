namespace EpinelPS.LobbyServer.Mission.Rewards;

[GameRequest("/mission/getrewarded/weekly")]
public class GetWeeklyRewards : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetWeeklyRewardedData>();
        User user = GetUser();

        user.ResetDataIfNeeded();

        ResGetWeeklyRewardedData response = new();
        response.Ids.Add(user.WeeklyResetableData.CompletedWeeklyMissions);

        await WriteDataAsync(response);
    }
}
