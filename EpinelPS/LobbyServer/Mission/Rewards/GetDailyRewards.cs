using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/daily")]
    public class GetDailyRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetDailyRewardedData>();
            var user = GetUser();

            user.ResetDataIfNeeded();

            var response = new ResGetDailyRewardedData();
            response.Ids.Add(user.ResetableData.CompletedDailyMissions);

            await WriteDataAsync(response);
        }
    }
}
