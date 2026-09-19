namespace EpinelPS.LobbyServer.Team;

[GameRequest("/team/get")]
public class GetTeamData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetTeamData req = await ReadData<ReqGetTeamData>();
        User user = GetUser();

        if (user.Characters.Count == 0 && user.LastNormalStageCleared >= 6000002)
        {
            Stage.ClearStage.EnsureDefaultCharacters(user);
        }

        ResGetTeamData response = new();

        // NOTE: Keep this in sync with EnterLobbyServer code
        if (user.Characters.Count > 0)
        {
            foreach (KeyValuePair<int, NetUserTeamData> item in user.UserTeams)
            {
                response.TypeTeams.Add(item.Value);
            }
        }
        await WriteDataAsync(response);
    }
}
