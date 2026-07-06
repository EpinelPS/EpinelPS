using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/upgrade")]
public class Upgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetArcadeTowerDefenseUpgrade req = await ReadData<ReqSetArcadeTowerDefenseUpgrade>();
        User user = GetUser();
        ResSetArcadeTowerDefenseUpgrade response = new();

        if (user.TowerDefenseDatas.TryGetValue(req.ArcadeManagerId, out TowerDefenseData? data))
        {
            EventTowerDefenseUpgradeListRecord? upcost = GameData.Instance.EventTowerDefenseUpgradeListTable.Values
                .Where(x => x.Id == req.UpgradeId).FirstOrDefault();
            data.UpgradeCurrency -= upcost.UpgradeCost;
            data.UpgradeIdList.AddUnique(req.UpgradeId);
            response.UpgradeCurrency = data.UpgradeCurrency;

        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}