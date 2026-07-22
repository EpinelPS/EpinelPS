using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/setcharacterunlockupgrade")]
public class SetCharacterUnlockUpgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetArcadeBubbleMarchCharacterUnlockUpgrade req = await ReadData<ReqSetArcadeBubbleMarchCharacterUnlockUpgrade>();
        User user = GetUser();
        ResSetArcadeBubbleMarchCharacterUnlockUpgrade response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {
            var unlckchar = GameData.Instance.EventBubbleMarchCharacterUnlockTable.Values
                .Where(x => x.Id == req.UpgradeId).FirstOrDefault();


            if (marchData.UpgradeCurrency.TryGetValue(unlckchar.UnlockCurrencyId, out var currdata))
            {
                currdata -= unlckchar.UnlockCurrencyValue;


                var progress = new BmHelper.MissionProgressData
                {
                    CostCurrency = (unlckchar.UnlockCurrencyId, unlckchar.UnlockCurrencyValue)                    
                };

                BmHelper.UpMission(marchData, MiniGameSystemType.Arcade, progress);

                marchData.CharacterUpgradeIdList.AddUnique(req.UpgradeId);
            }
            
            response.UpgradeCurrencyId = unlckchar.UnlockCurrencyId;
            response.UpgradeCurrencyValue = marchData.UpgradeCurrency[unlckchar.UnlockCharacterId];
            response.UpdatedAchievementMissionList.AddRange(MiniGameHelper
                .ToProtoDict<int, NetMiniGameBubbleMarchMissionData, MiniGameBubbleMarchMissionData>(marchData.AchievementMissionDataList).Values);
        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}