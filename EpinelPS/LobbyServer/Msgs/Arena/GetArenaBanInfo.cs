using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Msgs.Arena
{
    [PacketPath("/arena/getbaninfo")]
    public class GetArenaBanInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArenaBanInfo>();

            var response = new ResGetArenaBanInfo();
            response.RookieArenaBanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) };
            response.SpecialArenaBanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) };

            await WriteDataAsync(response);
        }
    }
}
