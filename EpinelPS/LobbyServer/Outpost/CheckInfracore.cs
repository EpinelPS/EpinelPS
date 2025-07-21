using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/infracore/check")]
    public class CheckInfracore : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCheckReceiveInfraCoreReward req = await ReadData<ReqCheckReceiveInfraCoreReward>();
            ResCheckReceiveInfraCoreReward response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
