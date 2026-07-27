using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/resetcharacterunlockupgrade")]
public class ResetCharacterUnlockUpgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqResetArcadeBubbleMarchCharacterUnlockUpgrade req = await ReadData<ReqResetArcadeBubbleMarchCharacterUnlockUpgrade>();
        User user = GetUser();
        ResResetArcadeBubbleMarchCharacterUnlockUpgrade response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
            .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {
            var bufflist = GameData.Instance.EventBubbleMarchCharacterUnlockTable.Values
                .Where(x => marchData.CharacterUpgradeIdList.Contains(x.Id))
                .ToList();
            var costSummary = bufflist
                .GroupBy(x => x.UnlockCurrencyId)
                .Select(g => new
                {
                    CurrencyId = g.Key,
                    TotalCost = g.Sum(x => x.UnlockCurrencyValue)
                })
                .ToList();

            foreach (var item in costSummary)
            {
                marchData.UpgradeCurrency[item.CurrencyId] =
                    marchData.UpgradeCurrency.GetValueOrDefault(item.CurrencyId) + item.TotalCost;
                response.UpgradeCurrencyId = item.CurrencyId;
                response.UpgradeCurrencyValue = marchData.UpgradeCurrency[item.CurrencyId];
            }

            marchData.CharacterUpgradeIdList.Clear();
           
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}