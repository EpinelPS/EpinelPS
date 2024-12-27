using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission
{
    [PacketPath("/mission/getrewarded/all")]
    public class GetAllRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetRewardedData>();
            var user = GetUser();

            var response = new ResGetRewardedData();

            response.AchievementIds.Add(user.CompletedAchievements);
            response.WeeklyIds.Add(user.WeeklyResetableData.CompletedWeeklyMissions);
            response.DailyIds.Add(user.ResetableData.CompletedDailyMissions);

            await WriteDataAsync(response);
        }
    }
}
