using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/enterlobbyserver")]
    public class EnterLobbyServer : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterLobbyServer>();
            var user = GetUser();

            // NOTE: Keep this in sync with GetUser code

            var response = new ResEnterLobbyServer();
            response.User = new NetUserData();
            response.User.Lv = 1;
            response.User.CommanderRoomJukebox = 5;
            response.User.CostumeLv = 1;
            response.User.Frame = 1;
            response.User.Icon = 39900;
            response.User.LobbyJukebox = 2;
            response.ResetHour = 20;
            response.Nickname = user.Nickname;
            response.SynchroLv = 1;
            response.OutpostBattleLevel = new NetOutpostBattleLevel() { Level = 1 };
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000 };

            if (user.TeamData.Slots.Count == 0)
            {
                user.TeamData = new NetWholeUserTeamData() { TeamNumber = 1, Type = 2 };
                user.TeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 1 });
                user.TeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 2 });
                user.TeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 3 });
                user.TeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 4 });
                user.TeamData.Slots.Add(new NetWholeTeamSlot() { Slot = 5 });
                JsonDb.Save();
            }
            response.RepresentationTeam = user.TeamData;

            foreach (var item in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }
            foreach (var item in user.Characters)
            {
                response.Character.Add(new NetUserCharacterData() { Default = new() { Csn = item.Csn, Skill1Lv = item.Skill1Lvl, Skill2Lv = item.Skill2Lvl, CostumeId = item.CostumeId, Lv = item.Level, Grade = item.Grade, Tid = item.Tid } });
            }

            if (user.Characters.Count > 0)
            {
                var team1 = new NetUserTeamData();
                team1.Type = 1;
                team1.LastContentsTeamNumber = 1;

                var team1Sub = new NetTeamData();
                team1Sub.TeamNumber = 1;

                // TODO: Save this properly. Right now return first 5 characters as a squad.
                for (int i = 1; i < 6; i++)
                {
                    var character = user.Characters[i - 1];
                    team1Sub.Slots.Add(new NetTeamSlot() { Slot = i, Value = character.Csn });
                }
                team1.Teams.Add(team1Sub);

                response.TypeTeams.Add(team1);
            }

            // TODO: Save outpost data
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 1, BuildingId = 22401, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 4, BuildingId = 22701, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 5, BuildingId = 22801, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 6, BuildingId = 22901, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 7, BuildingId = 23001, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 3, BuildingId = 23101, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 2, BuildingId = 23201, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 9, BuildingId = 23301, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 8, BuildingId = 23401, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 10, BuildingId = 23501, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });
            //response.Outposts.Add(new NetUserOutpostData() { SlotId = 38, BuildingId = 33601, IsDone = true, StartAt = 638549982076760660, CompleteAt = 638549982076760660 });

            response.LastClearedNormalMainStageId = user.LastStageCleared;

            // Restore completed tutorials. GroupID is the first 4 digits of the Table ID.
            foreach (var item in user.ClearedTutorials)
            {
                var groupId = int.Parse(item.ToString().Substring(0, 4));
                int tutorialVersion = item == 1020101 ? 1 : 0; // TODO
                response.User.Tutorials.Add(new NetTutorialData() { GroupId = groupId, LastClearedTid = item, LastClearedVersion = tutorialVersion });
            }

            WriteData(response);
        }
    }
}
