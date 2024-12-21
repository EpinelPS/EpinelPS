using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/mission/getrewarded/jukebox")]
    public class GetJukeboxRewards : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetJukeboxRewardedData>();

            // TODO: save these things
            var response = new ResGetJukeboxRewardedData();

            await WriteDataAsync(response);
        }
    }
}
