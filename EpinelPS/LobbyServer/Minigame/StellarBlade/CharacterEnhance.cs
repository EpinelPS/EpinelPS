using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/character/enhance")]
public class CharacterEnhance : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeEnhanceStellarBladeCharacter req = await ReadData<ReqArcadeEnhanceStellarBladeCharacter>();
        User user = GetUser();
        ResArcadeEnhanceStellarBladeCharacter response = new();

        EventSBManagerRecord_Raw? sbm = GameData.Instance.EventSBManagerTable.Values
            .Where(s => s.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            NetStellarBladeCharacterData.Types.NetEnhanceData? enhanceData = stellar.CharacterData.EnhanceDataList
                .FirstOrDefault(c => c.EnhanceType == req.EnhanceType);

            if (enhanceData != null)
            {
                
                EventSBCharacterEnhanceRecord_Raw? ent = GameData.Instance.EventSBCharacterEnhanceTable.Values
                    .Where(x =>x.GroupId == sbm.EnhanceGroupId && 
                    x.EnhanceType == (SBCharacterEnhanceType)req.EnhanceType && 
                    x.EnhanceLevel == (req.CurrentLevel + 1)).FirstOrDefault();

                var cost = stellar.Currency
                    .FirstOrDefault(c => c.CurrencyType == (int)ent.EnhanceMaterialItemType);

                cost.Amount -= ent.EnhanceMaterialItemValue;

                enhanceData.EnhanceLevel = req.CurrentLevel + 1;
                
            }
        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}