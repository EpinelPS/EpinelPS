using EpinelPS.LobbyServer.Soloraid;

namespace EpinelPS.LobbyServer.Badge;

[GameRequest("/badge/permanentcontent")]
public class PermanentContent : LobbyMessage
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
        int raidId = SoloRaidHelper.GetRaidId();
        var soloRaidData = SoloRaidHelper.GetSoloRaidData(user, raidId);
        response.TodaySoloRaidNormalPlayCount = soloRaidData?.RaidOpenCount ?? 0;

        await WriteDataAsync(response);
    }
}
