namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/scenario/exist")]
public class CheckScenarioExists : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqExistEventScenario req = await ReadData<ReqExistEventScenario>();

        ResExistEventScenario response = new();

        foreach (string? item in req.ScenarioGroupIds)
            response.ExistGroupIds.Add(item);


        await WriteDataAsync(response);
    }
}
