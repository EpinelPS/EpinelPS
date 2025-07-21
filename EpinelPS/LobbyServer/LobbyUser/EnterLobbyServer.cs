using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/enterlobbyserver")]
    public class EnterLobbyServer : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterLobbyServer req = await ReadData<ReqEnterLobbyServer>();
            User user = GetUser();

            TimeSpan battleTime = DateTime.UtcNow - user.BattleTime;
            long battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);

            // NOTE: Keep this in sync with GetUser code

            ResEnterLobbyServer response = new()
            {
                User = LobbyHandler.CreateNetUserDataFromUser(user),
                ResetHour = 20,
                Nickname = user.Nickname,
                SynchroLv = 1,
                OutpostBattleLevel = user.OutpostBattleLevel,
                OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs },

                Jukeboxv2 = new NetUserJukeboxDataV2() { CommandBgm = new NetJukeboxBgm() { JukeboxTableId = user.CommanderMusic.TableId, Type = NetJukeboxBgmType.JukeboxTableId, Location = NetJukeboxLocation.CommanderRoom } }
            };



            foreach (KeyValuePair<CurrencyType, long> item in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }

            foreach (Database.Character item in user.Characters)
            {
                response.Character.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = user.GetCharacterLevel(item.Csn, item.Level), Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel}, IsSynchro = user.GetSynchro(item.Csn) });
            }
          
            foreach (NetUserItemData item in NetUtils.GetUserItems(user))
            {
                response.Items.Add(item);
            }

            // Add squad data if there are characters
            if (user.Characters.Count > 0)
            {
                List<Database.Character> highestLevelCharacters = [.. user.Characters.OrderByDescending(x => x.Level).Take(5)];
                response.SynchroLv = user.GetSynchroLevel();

                foreach (Database.Character? item in highestLevelCharacters)
                {
                    response.SynchroStandardCharacters.Add(item.Csn);
                }

                foreach (KeyValuePair<int, NetUserTeamData> teamInfo in user.UserTeams)
                    response.TypeTeams.Add(teamInfo.Value);
            }

            // TODO: Save outpost data
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 1, BuildingId = 22401, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 4, BuildingId = 22701, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 5, BuildingId = 22801, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 6, BuildingId = 22901, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 7, BuildingId = 23001, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 3, BuildingId = 23101, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 2, BuildingId = 23201, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 9, BuildingId = 23301, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 8, BuildingId = 23401, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 10, BuildingId = 23501, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            response.Outposts.Add(new NetUserOutpostData() { SlotId = 38, BuildingId = 33601, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });

            response.LastClearedNormalMainStageId = user.LastNormalStageCleared;
            response.TimeRewardBuffs.AddRange(NetUtils.GetOutpostTimeReward(user));

            response.OwnedLobbyDecoBackgroundIdList.AddRange(user.LobbyDecoBackgroundList);

            response.ClearLessons.AddRange(user.CompletedTacticAcademyLessons);
            
            await WriteDataAsync(response);
        }
    }
}
