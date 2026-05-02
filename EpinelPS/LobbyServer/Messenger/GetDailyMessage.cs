namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/daily/pick")]
public class GetDailyMessage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqPickTodayDailyMessage req = await ReadData<ReqPickTodayDailyMessage>();

        // TODO: save these things
        ResPickTodayDailyMessage response = new();

        await WriteDataAsync(response);
    }
}
