namespace EpinelPS.LobbyServer.Intercept;

[GameRequest("/intercept/enter")]
public class EnterInterceptData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterIntercept req = await ReadData<ReqEnterIntercept>();
        User user = GetUser();

        ResEnterIntercept response = new();

        user.AddTrigger(Data.Trigger.InterceptStart, 1);

        await WriteDataAsync(response);
    }
}
