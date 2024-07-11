using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/showoutpostbattlereward")]
    public class ShowBattleReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqShowOutpostBattleReward>();
            var user = GetUser();

            var battleTime = DateTime.UtcNow - user.BattleTime;
            var battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);

            var response = new ResShowOutpostBattleReward();
            response.OutpostBattleLevel = user.OutpostBattleLevel;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs };
          await  WriteDataAsync(response);
        }
    }
}
