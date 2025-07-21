using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/weekly")]
    public class GetWeeklyRewards : LobbyMsgHandler
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
}
