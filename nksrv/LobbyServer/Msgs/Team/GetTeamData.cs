using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;

namespace nksrv.LobbyServer.Msgs.Team
{
    [PacketPath("/team/get")]
    public class GetTeamData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetTeamData>();
            var user = GetUser();

            var response = new ResGetTeamData();

            // NOTE: Keep this in sync with EnterLobbyServer code
            if (user.Characters.Count > 0)
            {
                foreach (var item in user.UserTeams)
                {
                    response.TypeTeams.Add(item.Value);
                }
            }
            await WriteDataAsync(response);
        }
    }
}
