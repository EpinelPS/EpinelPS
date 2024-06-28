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
            WriteData(response);
        }
    }
}
