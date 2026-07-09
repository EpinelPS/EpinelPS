using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/skill/reset")]
public class SkillReset : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeResetStellarBladeSkill req = await ReadData<ReqArcadeResetStellarBladeSkill>();
        User user = GetUser();
        ResArcadeResetStellarBladeSkill response = new();

        EventSBManagerRecord_Raw? sbm = GameData.Instance.EventSBManagerTable.Values
                    .Where(m => m.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();

        EventSBCharacterRecord_Raw? charrd = GameData.Instance.EventSBCharacterTable.Values
                .Where(c => c.Id == sbm.CharacterId).FirstOrDefault();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            List<int> learnedSkill = stellar.CharacterData.LearnedSkillIdList.ToList();

            int totalSkillPoints = learnedSkill
                .Select(id => GameData.Instance.EventSBCharacterSkillTable.Values.FirstOrDefault(s => s.Id == id))
                .Where(skill => skill != null)
                .Sum(skill => skill.NeedLearnSkillpoint);

            List<int>? learnskilllist = GameData.Instance.EventSBCharacterSkillTable.Values
               .Where(s => s.GroupId == charrd.SkillGroupId && s.IsDefaultLearn == true)
               .Select(s => s.Id)
               .ToList();

            stellar.CharacterData.LearnedSkillIdList.Clear();
            stellar.CharacterData.LearnedSkillIdList.AddRange(learnskilllist);

            NetStellarBladeCurrency? currency = stellar.Currency.FirstOrDefault(c => c.CurrencyType == (int)SBCurrencyType.SkillPoint);
            if (currency != null)
            {
                currency.Amount += totalSkillPoints;
            }
        }

        
        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}