using EpinelPS.Database;

using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Team;

[GameRequest("/team/setteam")]
public class SetTeam : LobbyMessage
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
        response.Teams.AddRange([.. req.Teams]);

        // Museum teams are scoped by stage. The client uses TeamType 31 and
        // sends MuseumStageTable.Id as ContentsId.
        if (req.Type == (int)TeamType.SoloRaidMuseum && req.ContentsId > 0)
        {
            if (!user.SoloRaidMuseumData.TryGetValue(req.ContentsId, out var museumStage))
            {
                museumStage = new SoloRaidMuseumStageData { StageId = req.ContentsId };
                user.SoloRaidMuseumData[req.ContentsId] = museumStage;
            }

            foreach (var newTeam in req.Teams)
            {
                var index = museumStage.Teams.FindIndex(x => x.TeamNumber == newTeam.TeamNumber);
                if (index >= 0)
                    museumStage.Teams[index] = newTeam.Clone();
                else
                    museumStage.Teams.Add(newTeam.Clone());
            }

            JsonDb.Save();
            await WriteDataAsync(response);
            return;
        }

        // Add team data to user data
        int contentsId = req.ContentsId + 1; // Default to 1 if not provided

        if (req.Teams.Count != 0)
        {
            contentsId = req.Teams.Select(x => x.TeamNumber).Max(x => x);
        }

        NetUserTeamData teamData = new() { LastContentsTeamNumber = contentsId, Type = req.Type };

        // Check for existing teams with same TeamNumber and replace or add accordingly
        foreach (var newTeam in req.Teams)
        {
            bool teamUpdated = false;
            for (int i = 0; i < teamData.Teams.Count; i++)
            {
                if (teamData.Teams[i].TeamNumber == newTeam.TeamNumber)
                {
                    // Replace existing team with same TeamNumber
                    teamData.Teams[i] = newTeam;
                    teamUpdated = true;
                    break;
                }
            }

            if (!teamUpdated)
            {
                // Add new team if TeamNumber doesn't exist
                teamData.Teams.Add(newTeam);
            }
        }

        if (!user.UserTeams.TryAdd(req.Type, teamData))
        {
            // If key already exists, we need to merge teams properly
            var existingTeamData = user.UserTeams[req.Type];
            existingTeamData.LastContentsTeamNumber = contentsId;
            existingTeamData.Type = req.Type;

            // Apply same logic to existing team data
            foreach (var newTeam in req.Teams)
            {
                bool teamUpdated = false;
                for (int i = 0; i < existingTeamData.Teams.Count; i++)
                {
                    if (existingTeamData.Teams[i].TeamNumber == newTeam.TeamNumber)
                    {
                        // Replace existing team with same TeamNumber
                        existingTeamData.Teams[i] = newTeam;
                        teamUpdated = true;
                        break;
                    }
                }

                if (!teamUpdated)
                {
                    // Add new team if TeamNumber doesn't exist
                    existingTeamData.Teams.Add(newTeam);
                }
            }
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
