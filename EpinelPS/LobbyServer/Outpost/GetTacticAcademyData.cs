namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/tactic/get")]
public class GetTacticAcademyData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetTacticAcademyData req = await ReadData<ReqGetTacticAcademyData>();
        User user = GetUser();

        ResGetTacticAcademyData response = new();
        response.ClearLessons.AddRange(user.CompletedTacticAcademyLessons);

        await WriteDataAsync(response);
    }
}
