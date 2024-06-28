using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Team
{
    [PacketPath("/team/setteam")]
    public class SetTeam : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetTeam>();
            var user = GetUser();

            var response = new ResSetTeam();
            response.Type = req.Type;
            response.Teams.AddRange(req.Teams.ToArray());

            // TODO

            WriteData(response);
        }
    }
}
