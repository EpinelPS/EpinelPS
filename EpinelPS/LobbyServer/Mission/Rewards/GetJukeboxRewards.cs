using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission.Rewards
{
    [PacketPath("/mission/getrewarded/jukebox")]
    public class GetJukeboxRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetJukeboxRewardedData req = await ReadData<ReqGetJukeboxRewardedData>();

            // TODO: save these things
            ResGetJukeboxRewardedData response = new();

            await WriteDataAsync(response);
        }
    }
}
