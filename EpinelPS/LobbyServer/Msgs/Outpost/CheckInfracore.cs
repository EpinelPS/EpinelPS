using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Outpost
{
    [PacketPath("/infracore/check")]
    public class CheckInfracore : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckReceiveInfraCoreReward>();
            var response = new ResCheckReceiveInfraCoreReward();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
