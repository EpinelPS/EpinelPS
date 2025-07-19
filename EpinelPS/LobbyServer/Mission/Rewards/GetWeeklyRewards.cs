using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/weekly")]
    public class GetWeeklyRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetWeeklyRewardedData>();
            var user = GetUser();

            user.ResetDataIfNeeded();

            var response = new ResGetWeeklyRewardedData();
            response.Ids.Add(user.WeeklyResetableData.CompletedWeeklyMissions);

            await WriteDataAsync(response);
        }
    }
}
