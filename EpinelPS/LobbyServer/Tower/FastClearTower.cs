using EpinelPS;
using EpinelPS.LobbyServer;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Tower;

[GameRequest("/tower/fastcleartower")]
public class FastClearTower : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFastClearTower req = await ReadData<ReqFastClearTower>();

        ResFastClearTower response = new();

        User user = GetUser();

        response.Reward = ClearTower.CompleteTower(user, req.TowerId).Reward;

        await WriteDataAsync(response);
    }
}

