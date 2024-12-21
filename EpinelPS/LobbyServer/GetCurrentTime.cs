using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer
{
    [PacketPath("/now")]
    public class GetCurrentTime : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetNow>();

            var response = new ResGetNow();
            response.Tick = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            response.ResetHour = 1;
            response.CheatShiftDuration = Duration.FromTimeSpan(TimeSpan.FromSeconds(0));
            // todo: validate response with actual server

            await WriteDataAsync(response);
        }
    }
}
