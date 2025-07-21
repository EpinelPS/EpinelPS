using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena
{
    [PacketPath("/arena/get")]
    public class GetArena : LobbyMsgHandler
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


            await WriteDataAsync(response);
        }
    }
}
