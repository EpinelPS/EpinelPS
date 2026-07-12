using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/enterlobbyserver")]
public class EnterLobbyServer : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterLobbyServer req = await ReadData<ReqEnterLobbyServer>();
        User user = GetUser();
        var userDB = GameContext.Instance.Users.Find((ulong)UserId);

        TimeSpan battleTime = DateTime.UtcNow - user.BattleTime;
        long battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);

        // NOTE: Keep this in sync with GetUser code
        if (userDB.Nickname == null)
        {
            GameContext.Users.Where(u => u.ID == UserId).ExecuteUpdate(setters => setters.SetProperty(u => u.Nickname, "Player"));
        }

        ResEnterLobbyServer response = new()
        {
            User = LobbyHandler.CreateNetUserDataFromUser(user),
            ResetHour = JsonDb.Instance.ResetHourUtcTime,
            Nickname = userDB.Nickname,
            SynchroLv = 1,
            OutpostBattleLevel = user.OutpostBattleLevel,
            OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs },

            Jukeboxv2 = new NetUserJukeboxDataV2() { CommandBgm = new NetJukeboxBgm() { JukeboxTableId = user.CommanderMusic.TableId, Type = NetJukeboxBgmType.JukeboxTableId, Location = NetJukeboxLocation.CommanderRoom } }
        };



        foreach (KeyValuePair<CurrencyType, long> item in user.Currency)
        {
            response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
        }

        foreach (CharacterModel item in user.Characters)
        {
            response.Character.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = user.GetCharacterLevel(item.Csn, item.Level), Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel }, IsSynchro = user.GetSynchro(item.Csn) });
        }

        foreach (NetUserItemData item in NetUtils.GetUserItems(user))
        {
            response.Items.Add(item);
        }

        // Add squad data if there are characters
        if (user.Characters.Count > 0)
        {
            List<CharacterModel> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];
            response.SynchroLv = user.GetSynchroLevel();

            foreach (CharacterModel? item in highestLevelCharacters)
            {
                response.SynchroStandardCharacters.Add(item.Csn);
            }

            foreach (KeyValuePair<int, NetUserTeamData> teamInfo in user.UserTeams)
                response.TypeTeams.Add(teamInfo.Value);
        }

        if (user.OutpostBuildings != null && user.OutpostBuildings.Count > 0)
        {
            bool needsSave = false;
            foreach (NetUserOutpostData building in user.OutpostBuildings)
            {

                if (!building.IsDone && DateTime.UtcNow.Ticks >= building.CompleteAt)
                {
                    building.IsDone = true;
                    needsSave = true;
                }
            }

            if (needsSave)
            {
                JsonDb.Save();
            }

            response.Outposts.AddRange(user.OutpostBuildings);
        }

        response.LastClearedNormalMainStageId = user.LastNormalStageCleared;
        response.LastClearedStoryStageId = user.LastStoryStageCleared;
        response.LastClearedHardMainStageId = user.LastHardStageCleared;
        response.LastClearedMod = user.LastClearedDifficulty;

        response.TimeRewardBuffs.AddRange(NetUtils.GetOutpostTimeReward(user));

        response.OwnedLobbyDecoBackgroundIdList.AddRange(user.LobbyDecoBackgroundList);

        response.ClearLessons.AddRange(user.CompletedTacticAcademyLessons);

        await WriteDataAsync(response);
    }
}