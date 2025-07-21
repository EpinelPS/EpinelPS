using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer
{
    [PacketPath("/now")]
    public class GetCurrentTime : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetNow req = await ReadData<ReqGetNow>();

            ResGetNow response = new()
            {
                Tick = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ResetHour = 1,
                CheatShiftDuration = Duration.FromTimeSpan(TimeSpan.FromSeconds(0))
            };
            // todo: validate response with actual server

            await WriteDataAsync(response);
        }
    }
}
