using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena
{
    [PacketPath("/arena/special/showreward")]
    public class ShowSpecialArenaReward : LobbyMsgHandler
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
}
