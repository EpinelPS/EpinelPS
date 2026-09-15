using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Org.BouncyCastle.Ocsp;

namespace EpinelPS.LobbyServer.Tower;

[GameRequest("/tower/cleartower")]
public class ClearTower : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearTower req = await ReadData<ReqClearTower>();

        ResClearTower response = new();
        User user = GetUser();

        if (req.BattleResult == 1) response.Reward = TowerHelper.CompleteTower(user, req.TowerId);

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}