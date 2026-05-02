using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena;

[GameRequest("/arena/special/showreward")]
public class ShowSpecialArenaReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqShowSpecialArenaReward req = await ReadData<ReqShowSpecialArenaReward>();

        ResShowSpecialArenaReward response = new()
        {
            IsBan = true,
            BanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) }
        };
        await WriteDataAsync(response);
    }
}
