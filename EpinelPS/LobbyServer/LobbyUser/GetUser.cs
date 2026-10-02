using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/User/Get")]
public class GetUser : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetUserData req = await ReadData<ReqGetUserData>();
        ResGetUserData response = new();
        User user = GetUser();

        TimeSpan battleTime = DateTime.UtcNow - user.BattleTime;
        long battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);


        response.User = LobbyHandler.CreateNetUserDataFromUser(user);
        response.ResetHour = JsonDb.Instance.ResetHourUtcTime;
        response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs };
        response.OutpostBattleLevel = user.OutpostBattleLevel;
        response.IsSimple = req.IsSimple;
        // Auto-affiliate with Counters union (GSN 10001) so the Union Shop is unlocked for all players without manual edits
        long gsn = user.Guild?.guildId ?? 0;
        if (gsn <= 0)
        {
            gsn = 10001;
            user.Guild ??= new GuildData();
            user.Guild.guildId = 10001;
            user.Guild.LeaveAt = 0;
            JsonDb.Save();
        }
        response.Gsn = gsn;

        // Automatically ensure player has Union Chips (GuildCoin) for the Union Shop (at least 100,000)
        const long defaultUnionChips = 100000;
        if (user.GetCurrencyVal(CurrencyType.GuildCoin) < defaultUnionChips)
        {
            user.Currency[CurrencyType.GuildCoin] = defaultUnionChips;
            JsonDb.Save();
        }


        foreach (KeyValuePair<CurrencyType, long> item in user.Currency)
        {
            response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
        }
        response.RepresentationTeam = NetUtils.GetDisplayedTeam(user);

        response.LastClearedNormalMainStageId = user.LastNormalStageCleared;
        response.LastClearedStoryStageId = user.LastStoryStageCleared;
        response.LastClearedHardMainStageId = user.LastHardStageCleared;
        response.LastClearedMod = user.LastClearedDifficulty;

        // Restore completed tutorials. GroupID is the first 4 digits of the Table ID.
        foreach (KeyValuePair<int, ClearedTutorialData> item in user.ClearedTutorialDataNew)
        {
            response.User.Tutorials.Add(new NetTutorialData()
            {
                GroupId = item.Key,
                LastClearedTid = item.Value.Id,
                LastClearedVersion = item.Value.VersionGroup
            });
        }

        response.CommanderRoomJukeboxBgm = JukeboxUtils.BuildCurrentBgm(user, NetJukeboxLocation.CommanderRoom);
        response.LobbyJukeboxBgm = JukeboxUtils.BuildCurrentBgm(user, NetJukeboxLocation.Lobby);

        await WriteDataAsync(response);
    }
}
