using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/setbuffupgrade")]
public class SetBuffUpgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetArcadeBubbleMarchBuffUpgrade req = await ReadData<ReqSetArcadeBubbleMarchBuffUpgrade>();
        User user = GetUser();
        ResSetArcadeBubbleMarchBuffUpgrade response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
                     .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {
            var buff = GameData.Instance.EventBubbleMarchBuffTable.Values
                .Where(x => x.Id == req.UpgradeId)
                .FirstOrDefault();

            if (marchData.UpgradeCurrency.TryGetValue(buff.UpgradeCostCurrencyId,out var currdata))
            {
                currdata -= buff.UpgradeCostCurrencyValue;


                var progress = new BmHelper.MissionProgressData
                {                    
                    CostCurrency = (buff.UpgradeCostCurrencyId, buff.UpgradeCostCurrencyValue),
                    BuffLevel = buff.BuffLevel
                };

                BmHelper.UpMission(marchData, MiniGameSystemType.Arcade, progress);
            }

            marchData.BuffUpgradeIdList.AddUnique(req.UpgradeId);
            response.UpgradeCurrencyId = buff.UpgradeCostCurrencyId;
            response.UpgradeCurrencyValue = marchData.UpgradeCurrency[buff.UpgradeCostCurrencyId];
            response.UpdatedAchievementMissionList.AddRange(MiniGameHelper
                .ToProtoDict<int, NetMiniGameBubbleMarchMissionData, MiniGameBubbleMarchMissionData>(marchData.AchievementMissionDataList).Values); 
        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}