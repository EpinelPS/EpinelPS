using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena
{
    [PacketPath("/arena/champion/getbadgedata")]
    public class ChampionBadgeData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetChampionArenaDataByBadge>();
            var response = new ResGetChampionArenaDataByBadge();
           
            // TODO
            response.Schedule = new NetChampionArenaSchedule();
            response.NextSchedule = new NetChampionArenaSchedule();
            response.ChampionArenaContentsState = ChampionArenaContentsState.ChampionArenaContentsStateClosed;
            response.CurrentOrLastSeasonStartAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(5));

            await WriteDataAsync(response);
        }
    }
}
