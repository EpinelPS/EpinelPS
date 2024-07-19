using Google.Protobuf.WellKnownTypes;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Simroom
{
    [PacketPath("/simroom/get")]
    public class GetSimRoomData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSimRoom>();

            var response = new ResGetSimRoom()
            {
                OverclockData = new()
                {
                    CurrentSeasonData = new()
                    {
                        SeasonStartDate = Timestamp.FromDateTimeOffset(DateTime.UtcNow),
                        SeasonEndDate = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(7))
                    },
                    CurrentSeasonHighScore = new(),
                    CurrentSubSeasonHighScore = new(),
                    LatestOption = new()
                },
                NextLegacyBuffResetDate = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(7))
            };
            // TODO
            await WriteDataAsync(response);
        }
    }
}
