using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Tower;

[GameRequest("/tower/entertower")]
public class EnterTower : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterTower req = await ReadData<ReqEnterTower>();
        User user = GetUser();

        ResEnterTower response = new();


        user.AddTrigger(Trigger.TowerAllStart, 1);
        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
