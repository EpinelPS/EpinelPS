﻿using EpinelPS.Database;
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
                existingTeamData.LastContentsTeamNumber = req.ContentsId + 1;
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
}