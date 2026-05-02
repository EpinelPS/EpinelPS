using EpinelPS.Database;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer;

[GameRequest("/now")]
public class GetCurrentTime : LobbyMessage
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
