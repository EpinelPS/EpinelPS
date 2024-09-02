using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Msgs.Arena
{
    [PacketPath("/arena/get")]
    public class GetArena : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArena>();
            var user = GetUser();

            var response = new ResGetArena();
            response.BanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) };
            response.User = new NetArenaData() {User = LobbyHandler.CreateWholeUserDataFromDbUser(user) };


            await WriteDataAsync(response);
        }
    }
}
