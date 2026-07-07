using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/buy")]
public class Buy : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeBuyStellarBladeShopItem req = await ReadData<ReqArcadeBuyStellarBladeShopItem>();
        User user = GetUser();
        ResArcadeBuyStellarBladeShopItem response = new();

        EventSBManagerRecord_Raw? sbm = GameData.Instance.EventSBManagerTable.Values
            .Where(s => s.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            EventSBShopRecord_Raw? shop = GameData.Instance.EventSBShopTable.Values
                .Where(x => x.Id == req.ShopId).FirstOrDefault();
            if (shop!=null)
            {
                NetStellarBladeCurrency? gold = stellar.Currency.First(x => x.CurrencyType == (int)SBCurrencyType.Gold);
                gold.Amount -= shop.ItemPrice;

                stellar.SbItemIdList.AddUnique(shop.ItemId);
                MiniGameHelper.UpMission(ref stellar, missionType: SBMissionType.BuyItem, missionGroupId: sbm.MissionGroupId, buyCount: 1);
            }        
        
        }
        
        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}