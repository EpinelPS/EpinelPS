using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/getoutpostdata")]
    public class GetOutpostData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetOutpostData>();
            var user = GetUser();

            var battleTime = DateTime.UtcNow - user.BattleTime;
            var battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);


            var response = new ResGetOutpostData
            {
                OutpostBattleLevel = new NetOutpostBattleLevel() { Level = 1 },
                CommanderBgm = new NetUserJukeboxDataV2() { CommandBgm = new() { Type = NetJukeboxBgmType.JukeboxTableId, JukeboxTableId = 3012 } },
                BattleTime = 864000000000, Jukebox = new(), MaxBattleTime = 864000000000
            };

            response.OutpostBattleLevel = user.OutpostBattleLevel;
            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs };

       
            // TODO
            WriteData(response);
        }
    }
}
