using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/resetbuffupgrade")]
public class ResetBuffUpgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqResetArcadeBubbleMarchBuffUpgrade req = await ReadData<ReqResetArcadeBubbleMarchBuffUpgrade>();
        User user = GetUser();
        ResResetArcadeBubbleMarchBuffUpgrade response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {
            var bufflist = GameData.Instance.EventBubbleMarchBuffTable.Values
                .Where(x => marchData.BuffUpgradeIdList.Contains(x.Id))
                .ToList();
            var costSummary = bufflist
                .GroupBy(x => x.UpgradeCostCurrencyId)
                .Select(g => new
                {
                    CurrencyId = g.Key,
                    TotalCost = g.Sum(x => x.UpgradeCostCurrencyValue)
                })
                .ToList();

            foreach (var item in costSummary)
            {                
                marchData.UpgradeCurrency[item.CurrencyId] =
                    marchData.UpgradeCurrency.GetValueOrDefault(item.CurrencyId) + item.TotalCost;
                response.UpgradeCurrencyId = item.CurrencyId;
                response.UpgradeCurrencyValue = marchData.UpgradeCurrency[item.CurrencyId];
            }

            marchData.BuffUpgradeIdList.Clear();           
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}