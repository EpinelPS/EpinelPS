namespace EpinelPS.LobbyServer.Misc;

[GameRequest("/lobby/retroactive")]
public class LobbyRetroactive : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRetroactive req = await ReadData<ReqRetroactive>();
        User user = GetUser();

        ResRetroactive response = new();
        await WriteDataAsync(response);
    }
}
