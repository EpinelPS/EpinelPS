using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Org.BouncyCastle.Asn1.Pkcs;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/buff/reset")]
public class BuffReset : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqResetMiniGameIslandBreakerBuff req = await ReadData<ReqResetMiniGameIslandBreakerBuff>();
        User user = GetUser();
        ResResetMiniGameIslandBreakerBuff response = new();

        if (user.IsLandBreakerDatas.TryGetValue(req.IslandBreakerId, out var isLandData))
        {
            var bufflist = GameData.Instance.EventIslandBreakerBuffTable.Values
               .Where(x => isLandData.Buffs.Contains(x.Id))
               .ToList();
            int totalCost = bufflist.Sum(x => x.BuffCurrencyCount);

            isLandData.Buffs.Clear();
            isLandData.Buffs.AddRange(bufflist.Where(x => x.BuffLevel == 0).Select(x => x.Id).ToList());
            
            isLandData.Currencies[2].CurrentAmount = Math.Min(
                isLandData.Currencies[2].CurrentAmount + totalCost,
                isLandData.Currencies[2].MaxLimit
            );

            response.NewBuffCurrencyAmount = isLandData.Currencies[2].CurrentAmount;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}