using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/User/Get")]
    public class GetUser : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetUserData>();

            
            var response = new ResGetUserData();

            var user = GetUser();

            response.User = new NetUserData();
            response.User.Lv = 1;
            response.User.CommanderRoomJukebox = 5;
            response.User.CostumeLv = 1;
            response.User.Frame = 1;
            response.User.Icon = user.ProfileIconId;
            response.User.IconPrism = user.ProfileIconIsPrism;
            response.User.LobbyJukebox = 2;
            response.ResetHour = 20;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000 };
            response.IsSimple = req.IsSimple;

            foreach (var item in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }
            response.RepresentationTeam = user.TeamData;

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
