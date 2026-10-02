using EpinelPS.Database;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena;

[GameRequest("/arena/get")]
public class GetArena : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArena req = await ReadData<ReqGetArena>();
        User user = GetUser();

        ResGetArena response = new()
        {
            BanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) },
            User = new NetArenaData() { User = LobbyHandler.CreateWholeUserDataFromDbUser(user) }
        };

        // Advance Rookie Arena play count for Daily (10018, needs 2) and Weekly (20016, needs 10)
        user.AddTrigger(Data.Trigger.RookieArenaPlayCount, 2);
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
