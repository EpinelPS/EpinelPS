using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/weekly")]
    public class GetWeeklyRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetWeeklyRewardedData>();

            // TODO: implement
            var response = new ResGetWeeklyRewardedData();

            await WriteDataAsync(response);
        }
    }
}
