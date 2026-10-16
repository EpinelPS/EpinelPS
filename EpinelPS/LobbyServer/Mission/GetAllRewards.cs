namespace EpinelPS.LobbyServer.Mission;

[GameRequest("/mission/getrewarded/all")]
public class GetAllRewards : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetRewardedData req = await ReadData<ReqGetRewardedData>();
        User user = GetUser();

        // Ensure daily and weekly reset timestamps are honored before returning claimed lists
        user.ResetDataIfNeeded();
        MissionReconciler.ReconcileAll(user);

        ResGetRewardedData response = new();

        response.AchievementIds.Add(user.CompletedAchievements);
        response.WeeklyIds.Add(user.WeeklyResetableData.CompletedWeeklyMissions);
        response.DailyIds.Add(user.ResetableData.CompletedDailyMissions);

        await WriteDataAsync(response);
    }
}
