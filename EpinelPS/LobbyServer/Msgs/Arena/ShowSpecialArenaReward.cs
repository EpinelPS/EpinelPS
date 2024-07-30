using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Arena
{
    [PacketPath("/arena/special/showreward")]
    public class ShowSpecialArenaReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqShowSpecialArenaReward>();

            var response = new ResShowSpecialArenaReward();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
