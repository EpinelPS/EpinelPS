using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;
using EpinelPS.Database;

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
                ResetHour = JsonDb.Instance.ResetHourUtcTime,
                CheatShiftDuration = Duration.FromTimeSpan(TimeSpan.FromSeconds(0))
            };
            // todo: valIdate response with actual server

            await WriteDataAsync(response);
        }
    }
}
