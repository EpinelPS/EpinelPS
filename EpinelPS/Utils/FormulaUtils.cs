using EpinelPS.StaticInfo;

namespace EpinelPS.Utils
{
    public class FormulaUtils
    {
        public static int CalculateCP(Database.User user, long csn)
        {
            var character = user.Characters.FirstOrDefault(c => c.Csn == csn);
            var charRecord = GameData.Instance.characterTable.Values.FirstOrDefault(c => c.id == character.Tid);
            var statRecord = GameData.Instance.characterStatTable.Values.FirstOrDefault(s => charRecord.stat_enhance_id == s.group + (character.Level - 1));
            float coreMult = 1f + character.Grade * 0.02f;
            float hp = statRecord.level_hp * coreMult;
            float atk = statRecord.level_attack * coreMult;
            float def = statRecord.level_defence * coreMult;
            float critRate = charRecord.critical_ratio;
            float critDamage = charRecord.critical_damage;
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
