using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Mission
{
    [PacketPath("/mission/getrewarded/all")]
    public class GetAllRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetRewardedData>();

            var response = new ResGetRewardedData();

            // TOOD

            await WriteDataAsync(response);
        }
    }
}
