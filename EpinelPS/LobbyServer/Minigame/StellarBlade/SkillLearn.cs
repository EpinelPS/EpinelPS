using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using static Google.Protobuf.Reflection.FieldOptions.Types;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/skill/learn")]
public class SkillLearn : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeLearnStellarBladeSkill req = await ReadData<ReqArcadeLearnStellarBladeSkill>();
        User user = GetUser();
        ResArcadeLearnStellarBladeSkill response = new();

        EventSBManagerRecord_Raw? sbm = GameData.Instance.EventSBManagerTable.Values
            .Where(s => s.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            EventSBCharacterSkillRecord_Raw? skilltable = GameData.Instance.EventSBCharacterSkillTable.Values
                .Where(x=>x.Id == req.SkillId).FirstOrDefault();
            StellarBladeCurrency? currency = stellar.Currency.FirstOrDefault(c => c.CurrencyType == (int)SBCurrencyType.SkillPoint);
            if (currency != null)
            {
                currency.Amount -= skilltable.NeedLearnSkillpoint;
            }

            List<int> list = stellar.CharacterData.LearnedSkillIdList.ToList();
            list.AddUnique(req.SkillId);
            stellar.CharacterData.LearnedSkillIdList.Clear();
            stellar.CharacterData.LearnedSkillIdList.AddRange(list);            
        }
        
        
        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}