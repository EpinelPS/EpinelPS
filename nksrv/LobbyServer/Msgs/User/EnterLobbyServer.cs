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
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000 ,MaxOverBattleTime = 12096000000000 };
            response.RepresentationTeam = new NetWholeUserTeamData() { TeamNumber = 1, Type = 2};
            response.RepresentationTeam.Slots.Add(new NetWholeTeamSlot() { Slot = 1 });
            response.RepresentationTeam.Slots.Add(new NetWholeTeamSlot() { Slot = 2 });
            response.RepresentationTeam.Slots.Add(new NetWholeTeamSlot() { Slot = 3 });
            response.RepresentationTeam.Slots.Add(new NetWholeTeamSlot() { Slot = 4 });
            response.RepresentationTeam.Slots.Add(new NetWholeTeamSlot() { Slot = 5 });

            foreach (var item in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }
          
            response.LastClearedNormalMainStageId = user.LastStageCleared;
            //var tTeams = new NetUserTeamData();

            //var tTeam = new NetTeamData() { TeamNumber = 1 };
            //tTeam.Slots.Add(new NetTeamSlot() { Slot = 1, ValueType = 1, Value = 2 });
            //tTeams.Teams.Add(tTeam);
            //response.TypeTeams.Add(tTeams);
           // response.Character.Add(new NetUserCharacterData() { Default = new NetUserCharacterDefaultData() { Tid = 1 } });

            WriteData(response);
        }
    }
}
