using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena
{
    [PacketPath("/arena/champion/getbadgedata")]
    public class ChampionBadgeData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetChampionArenaDataByBadge req = await ReadData<ReqGetChampionArenaDataByBadge>();
            ResGetChampionArenaDataByBadge response = new()
            {
                // TODO
                Schedule = new NetChampionArenaSchedule(),
                NextSchedule = new NetChampionArenaSchedule(),
                ChampionArenaContentsState = ChampionArenaContentsState.SeasonClosed,
                CurrentOrLastSeasonStartAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5))
            };

            await WriteDataAsync(response);
        }
    }
}
