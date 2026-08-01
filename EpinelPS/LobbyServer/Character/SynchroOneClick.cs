using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using System.Linq;

namespace EpinelPS.LobbyServer.Character;

[GameRequest("/character/synchrodevice/oneclick")]
public class SynchroOneClick : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSynchroOneClick req = await ReadData<ReqSynchroOneClick>();
        User user = GetUser();
        ResSynchroOneClick response = new();
        Dictionary<int, CharacterLevelRecord> data = GameData.Instance.GetCharacterLevelUpData();
        int maxLv = data.Keys.Count > 0 ? data.Keys.Max() : 0;

        foreach (NetSynchroCharacter sc in req.Chars)
        {
            CharacterModel? character = user.Characters.FirstOrDefault(c => c.Csn == sc.Csn);
            if (character == null) continue;

            int targetLv = sc.Lv;
            if (targetLv > maxLv) targetLv = maxLv;
            if (targetLv <= character.Level) continue;

            int requiredCredit = 0;
            int requiredBattleData = 0;
            int requiredCoreDust = 0;
            bool valid = true;
            for (int i = character.Level; i < targetLv; i++)
            {
                if (!data.TryGetValue(i, out var levelUpData)) { valid = false; break; }
                requiredCredit += levelUpData.Gold;
                requiredBattleData += levelUpData.CharacterExp;
                requiredCoreDust += levelUpData.CharacterExp2;
            }
            if (!valid) continue;

            if (user.CanSubtractCurrency(CurrencyType.Gold, requiredCredit) &&
                user.CanSubtractCurrency(CurrencyType.CharacterExp, requiredBattleData) &&
                user.CanSubtractCurrency(CurrencyType.CharacterExp2, requiredCoreDust))
            {
                user.SubtractCurrency(CurrencyType.Gold, requiredCredit);
                user.SubtractCurrency(CurrencyType.CharacterExp, requiredBattleData);
                user.SubtractCurrency(CurrencyType.CharacterExp2, requiredCoreDust);
                character.Level = targetLv;
            }
            else
            {
                continue;
            }

            response.Characters.Add(new NetUserCharacterDefaultData()
            {
                Csn = character.Csn,
                Tid = character.Tid,
                Lv = character.Level,
                Grade = character.Grade,
                CostumeId = character.CostumeId,
                UltiSkillLv = character.UltimateLevel,
                Skill1Lv = character.Skill1Lvl,
                Skill2Lv = character.Skill2Lvl,
            });
        }

        response.SynchroLv = user.GetSynchroLevel();
        foreach (CharacterModel c in user.Characters.OrderByDescending(x => x.Level).Take(5))
            response.SynchroCharacters.Add(c.Csn);

        foreach (KeyValuePair<CurrencyType, long> currency in user.Currency)
            response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });

        user.AddTrigger(Trigger.CharacterLevelUpCount, response.Characters.Count);
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
