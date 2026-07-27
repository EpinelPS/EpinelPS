using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/buff/upgrade")]
public class BuffUpgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUpgradeMiniGameIslandBreakerBuff req = await ReadData<ReqUpgradeMiniGameIslandBreakerBuff>();
        User user = GetUser();
        ResUpgradeMiniGameIslandBreakerBuff response = new();

        if (user.IsLandBreakerDatas.TryGetValue(req.IslandBreakerId, out var isLandData))
        {
            isLandData.Buffs.AddUnique(req.UpgradeBuffId);           

            EventIslandBreakerBuffRecord_Raw? buff = GameData.Instance.EventIslandBreakerBuffTable.Values
               .Where(x => x.Id== req.UpgradeBuffId)
               .FirstOrDefault();
            isLandData.Currencies[2].CurrentAmount -= buff.BuffCurrencyCount;
            response.NewBuffId = req.UpgradeBuffId;
            response.NewBuffCurrencyAmount = isLandData.Currencies[2].CurrentAmount;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}