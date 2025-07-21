using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena
{
    [PacketPath("/arena/special/get")]
    public class GetSpecialArena : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetSpecialArena req = await ReadData<ReqGetSpecialArena>();
            User user = GetUser();

            ResGetSpecialArena response = new()
            {
                BanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) },
                User = new NetArenaData() { User = LobbyHandler.CreateWholeUserDataFromDbUser(user) }
            };

            await WriteDataAsync(response);
        }
    }
}
