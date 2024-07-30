using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/daily")]
    public class GetDailyRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetDailyRewardedData>();

            // TODO: implement
            var response = new ResGetDailyRewardedData();

            await WriteDataAsync(response);
        }
    }
}
