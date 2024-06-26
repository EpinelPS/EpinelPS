using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Team
{
    [PacketPath("/team/get")]
    public class GetTeamData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetTeamData>();


            // TODO: assume that team data did not change
            var resp = new ResGetTeamData();
            //resp.TypeTeams
            WriteData(resp);
        }
    }
}
