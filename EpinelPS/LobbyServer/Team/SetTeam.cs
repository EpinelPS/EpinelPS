using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Team
{
    [PacketPath("/team/setteam")]
    public class SetTeam : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetTeam req = await ReadData<ReqSetTeam>();
            User user = GetUser();

            // TODO is this right
            ResSetTeam response = new()
            {
                Type = req.Type
            };
            response.Teams.AddRange(req.Teams.ToArray());

            // Add team data to user data
            NetUserTeamData teamData = new() { LastContentsTeamNumber = req.ContentsId + 1, Type = req.Type };
            teamData.Teams.AddRange(req.Teams);

            if (!user.UserTeams.TryAdd(req.Type, teamData))
            {
                user.UserTeams[req.Type] = teamData;
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
