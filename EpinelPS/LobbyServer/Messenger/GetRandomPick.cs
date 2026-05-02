namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/random/pick")]
public class GetRandomPick : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqPickTodayRandomMessage req = await ReadData<ReqPickTodayRandomMessage>();

        // TODO: get proper response
        ResPickTodayRandomMessage response = new();

        await WriteDataAsync(response);
    }
}
