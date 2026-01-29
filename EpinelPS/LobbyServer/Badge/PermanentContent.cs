using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Badge
{
    [PacketPath("/badge/permanentcontent")]
    public class PermanentContent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqPermanentContentBadgeData req = await ReadData<ReqPermanentContentBadgeData>();
            User user = GetUser();

            ResPermanentContentBadgeData response = new();
            response.ChampionArenaBadgeData = new();
            response.SoloRaidMuseumBadgeData = new();
            response.ChampionArenaBadgeData.Schedule = new();
            response.ChampionArenaBadgeData.NextSchedule = new();
            response.ChampionArenaBadgeData.ChampionArenaContentsState = ChampionArenaContentsState.SeasonClosed;
            response.ChampionArenaBadgeData.CurrentOrLastSeasonStartAt = new();
            // TODO

            await WriteDataAsync(response);
        }
    }
}
