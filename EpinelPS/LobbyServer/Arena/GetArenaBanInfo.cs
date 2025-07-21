using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena
{
    [PacketPath("/arena/getbaninfo")]
    public class GetArenaBanInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetArenaBanInfo req = await ReadData<ReqGetArenaBanInfo>();

            ResGetArenaBanInfo response = new()
            {
                RookieArenaBanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) },
                SpecialArenaBanInfo = new NetArenaBanInfo() { Description = "Not Implemented", StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow), EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddYears(10)) }
            };

            await WriteDataAsync(response);
        }
    }
}
