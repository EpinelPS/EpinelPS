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

            response.User = LobbyHandler.CreateNetUserDataFromUser(user);
            response.ResetHour = 20;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000 };
            response.IsSimple = req.IsSimple;

            foreach (var item in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }
            response.RepresentationTeam = user.RepresentationTeamData;

            response.LastClearedNormalMainStageId = user.LastNormalStageCleared;

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
