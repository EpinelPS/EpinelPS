namespace EpinelPS.LobbyServer.Tower;

[GameRequest("/tower/fastcleartower")]
public class FastClearTower : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFastClearTower req = await ReadData<ReqFastClearTower>();

        ResFastClearTower response = new();

        await WriteDataAsync(response);
    }
}
