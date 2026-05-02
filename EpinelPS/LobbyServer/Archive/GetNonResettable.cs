namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/scenario/getnonresettable")]
public class GetNonResettable : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetNonResettableArchiveScenario req = await ReadData<ReqGetNonResettableArchiveScenario>();
        ResGetNonResettableArchiveScenario response = new();

        User user = GetUser();
        foreach (var (evtId, evtData) in user.EventInfo)
        {
            if (evtId == req.EventId)
            {
                response.ScenarioIdList.AddRange(evtData.CompletedScenarios);
                break;
            }
        }

        await WriteDataAsync(response);
    }
}
