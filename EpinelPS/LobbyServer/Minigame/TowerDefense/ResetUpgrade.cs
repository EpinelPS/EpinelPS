using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;


namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/resetupgrade")]
public class ResetUpgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqResetArcadeTowerDefenseUpgrade req = await ReadData<ReqResetArcadeTowerDefenseUpgrade>();
        User user = GetUser();
        ResResetArcadeTowerDefenseUpgrade response = new();

        if (user.TowerDefenseDatas.TryGetValue(req.ArcadeManagerId, out TowerDefenseData? data))
        {
            List<int>? list = data.UpgradeIdList;
            if (list.Count>0)
            {
                int totalCost = GameData.Instance.EventTowerDefenseUpgradeListTable
                    .Where(x => list.Contains(x.Key))
                    .Sum(x => x.Value.UpgradeCost);
                data.UpgradeIdList = [];
                data.UpgradeCurrency += totalCost;
                response.UpgradeCurrency = data.UpgradeCurrency;
            }
        }
        
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}