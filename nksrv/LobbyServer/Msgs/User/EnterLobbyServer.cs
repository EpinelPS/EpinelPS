using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/enterlobbyserver")]
    public class EnterLobbyServer : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterLobbyServer>();
            var user = GetUser();

            var battleTime = DateTime.UtcNow - user.BattleTime;
            var battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);

            // NOTE: Keep this in sync with GetUser code

            var response = new ResEnterLobbyServer();
            response.User = LobbyHandler.CreateNetUserDataFromUser(user);
            response.ResetHour = 20;
            response.Nickname = user.Nickname;
            response.SynchroLv = 1;
            response.OutpostBattleLevel = user.OutpostBattleLevel;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs };
            response.CommanderRoomJukeboxBgm = new NetJukeboxBgm() { JukeboxTableId = 5, Type = NetJukeboxBgmType.JukeboxTableId, Location = NetJukeboxLocation.CommanderRoom };
            response.LobbyJukeboxBgm = new NetJukeboxBgm() { JukeboxTableId = 2, Type = NetJukeboxBgmType.JukeboxTableId, Location = NetJukeboxLocation.Lobby };

            // Add default slot data
            if (user.RepresentationTeamData.Slots.Count == 0)
            {
                user.RepresentationTeamData = new NetWholeUserTeamData() { TeamNumber = 1, Type = 2 };
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 2 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 3 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 4 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 5 });
                JsonDb.Save();
            }
            response.RepresentationTeam = user.RepresentationTeamData;

            foreach (var item in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }

            foreach (var item in user.Characters)
            {
                response.Character.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = item.Level, Grade = item.Grade, Tid = item.Tid, UltiSkillLv = item.UltimateLevel } });
            }

            foreach (var item in NetUtils.GetUserItems(user))
            {
                response.Items.Add(item);
            }

            // Add squad data if there are characters
            if (user.Characters.Count > 0)
            {
                var highestLevelCharacters = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();
                response.SynchroLv = highestLevelCharacters.Last().Level;
                foreach (var item in highestLevelCharacters)
                {
                    response.SynchroStandardCharacters.Add(item.Csn);
                }

                foreach (var teamInfo in user.UserTeams)
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

            await WriteDataAsync(response);
        }
    }
}
