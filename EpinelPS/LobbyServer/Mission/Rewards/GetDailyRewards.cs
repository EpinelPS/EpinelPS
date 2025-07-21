using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/daily")]
    public class GetDailyRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetDailyRewardedData>();
            Database.User user = GetUser();

            user.ResetDataIfNeeded();

            ResGetDailyRewardedData response = new();
            response.Ids.Add(user.ResetableData.CompletedDailyMissions);

            await WriteDataAsync(response);
        }
    }
}
