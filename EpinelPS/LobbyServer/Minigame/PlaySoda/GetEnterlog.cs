using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda
{
    [PacketPath("/arcade/enterlog")]
    public class GetEnterArcadeLog : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqEnterArcadeLog>();

            await WriteDataAsync(new ResEnterArcadeLog());
        }
    }
}
