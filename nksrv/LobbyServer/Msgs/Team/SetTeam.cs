using nksrv.Database;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Team
{
    [PacketPath("/team/setteam")]
    public class SetTeam : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetTeam>();
            var user = GetUser();

            // TODO is this right
            var response = new ResSetTeam();
            response.Type = req.Type;
            response.Teams.AddRange(req.Teams.ToArray());

            // Add team data to user data
            var teamData = new NetUserTeamData() { LastContentsTeamNumber = req.ContentsId, Type = req.Type };
            teamData.Teams.AddRange(req.Teams);

            if (user.UserTeams.ContainsKey(req.Type))
            {
                user.UserTeams[req.Type] = teamData;
            }
            else
            {
                user.UserTeams.Add(req.Type, teamData);
            }
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
