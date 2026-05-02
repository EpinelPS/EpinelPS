namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/boxgacha/execute")]
public class ExecuteEventBoxGacha : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // from client: {"EventId":10051,"CurrentCount":1}
        ReqExecuteEventBoxGacha req = await ReadData<ReqExecuteEventBoxGacha>();
        User user = GetUser();

        ResExecuteEventBoxGacha response = new()
        {

        };

        await WriteDataAsync(response);
    }
}