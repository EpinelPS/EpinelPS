using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena;

[GameRequest("/arena/special/get")]
public class GetSpecialArena : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetSpecialArena req = await ReadData<ReqGetSpecialArena>();
        GameUser gameUser = GetUserNew();

        ResGetSpecialArena response = new()
        {
            BanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) },
            User = new NetArenaData() { User = LobbyHandler.CreateWholeUserDataFromDbUser(gameUser) }
        };

        await WriteDataAsync(response);
    }
}
