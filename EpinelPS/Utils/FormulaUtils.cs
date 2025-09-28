using EpinelPS.Data;

namespace EpinelPS.Utils
{
    public class FormulaUtils
    {
        public static int CalculateCP(User user, long csn)
        {
            CharacterModel? character = user.Characters.FirstOrDefault(c => c.Csn == csn);
            if (character == null) return 0;

            CharacterRecord? charRecord = GameData.Instance.CharacterTable.Values.FirstOrDefault(c => c.Id == character.Tid);
            if (charRecord == null) return 0;

            CharacterStatRecord? statRecord = GameData.Instance.characterStatTable.Values.FirstOrDefault(s => charRecord.StatEnhanceId == s.Group + (character.Level - 1));
            if (statRecord == null) return 0;

            float coreMult = 1f + character.Grade * 0.02f;
            float hp = statRecord.LevelHp * coreMult;
            float atk = statRecord.LevelAttack * coreMult;
            float def = statRecord.LevelDefence * coreMult;
            float critRate = charRecord.CriticalRatio;
            float critDamage = charRecord.CriticalDamage;
            float skill1Level = character.Skill1Lvl;
            float skill2Level = character.Skill2Lvl;
            float ultSkillLevel = character.UltimateLevel;
            float critResult = 1 + ((critRate / 10000f) * (critDamage / 10000f - 1));
            float effHealthResult = (def * 100) + hp;
            float skillResult = (skill1Level * 0.01f) + (skill2Level * 0.01f) + (ultSkillLevel * 0.02f);
            float bondResult = 0f; // TODO
            float equipResult = 0f; // TOD
            float overloadResult = 0; // TODO
            float finalResult = (((critResult * atk * 18) + (effHealthResult * 0.7f)) * (1.3f + skillResult) + bondResult + equipResult + overloadResult) / 100f;

            return (int)Math.Round(finalResult);
        }
    }
}
