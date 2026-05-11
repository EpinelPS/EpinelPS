namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/scenario/exist")]
public class GetUserScenarioExist : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var user = GetUser();
        ReqExistScenario req = await ReadData<ReqExistScenario>();
        ResExistScenario response = new();

        foreach (string? item in req.ScenarioGroupIds)
        {
            if (user.CompletedScenarios.Contains(item) || user.EventInfo.Values.Any(x => x.CompletedScenarios.Contains(item)))
            {
                response.ExistGroupIds.Add(item);
            }
        }

        await WriteDataAsync(response);
    }
}
