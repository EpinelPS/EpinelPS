using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character;

internal class SkillLevelUpResult
{
    public NetUserCharacterDefaultData Character { get; set; } = new();
    public List<NetUserItemData> Items { get; } = [];
    public List<NetUserCurrencyData> Currencies { get; } = [];
}

internal static class SkillLevelUpHelper
{
    public static SkillLevelUpResult Upgrade(User user, long csn, CharacterSkillCategory category)
    {
        CharacterModel character = GetCharacter(user, csn);
        return Upgrade(user, csn, category, GetSkillLevel(character, category) + 1);
    }

    public static SkillLevelUpResult Upgrade(User user, long csn, CharacterSkillCategory category, int targetLevel)
    {
        CharacterModel character = GetCharacter(user, csn);

        if (category == CharacterSkillCategory.None)
        {
            throw new BadHttpRequestException("Skill category is required", 400);
        }

        int currentLevel = GetSkillLevel(character, category);
        if (targetLevel <= currentLevel)
        {
            return new SkillLevelUpResult { Character = ToNetCharacter(character) };
        }

        if (targetLevel > 10)
        {
            throw new BadHttpRequestException($"Invalid target skill level {targetLevel}", 400);
        }

        int levelUpCount = targetLevel - currentLevel;
        int baseSkillId = GetBaseSkillId(character, category);
        Dictionary<int, int> itemCosts = [];
        Dictionary<CurrencyType, long> currencyCosts = [];

        for (int level = currentLevel; level < targetLevel; level++)
        {
            int skillId = baseSkillId + level - 1;
            if (!GameData.Instance.skillInfoTable.TryGetValue(skillId, out SkillInfoRecord? skillRecord))
            {
                throw new BadHttpRequestException($"Skill info {skillId} not found", 400);
            }

            if (skillRecord.LevelUpCostId == 0)
            {
                continue;
            }

            if (!GameData.Instance.costTable.TryGetValue(skillRecord.LevelUpCostId, out CostRecord? costRecord))
            {
                throw new BadHttpRequestException($"Skill cost {skillRecord.LevelUpCostId} not found", 400);
            }

            AddCosts(costRecord, itemCosts, currencyCosts);
        }

        EnsureEnoughResources(user, itemCosts, currencyCosts);

        SkillLevelUpResult result = new();
        DeductResources(user, itemCosts, currencyCosts, result);

        SetSkillLevel(character, category, targetLevel);
        result.Character = ToNetCharacter(character);

        user.AddTrigger(Trigger.CharacterSkillLevelUpCount, levelUpCount);
        if (character.UltimateLevel == 10 && character.Skill1Lvl == 10 && character.Skill2Lvl == 10)
        {
            user.AddTrigger(Trigger.CharacterSkillLevelMax, 1);
        }

        return result;
    }

    private static CharacterModel GetCharacter(User user, long csn)
    {
        return user.Characters.FirstOrDefault(c => c.Csn == csn)
            ?? throw new BadHttpRequestException($"Character with CSN {csn} not found", 404);
    }

    private static int GetBaseSkillId(CharacterModel character, CharacterSkillCategory category)
    {
        if (!GameData.Instance.CharacterTable.TryGetValue(character.Tid, out CharacterRecord? charRecord))
        {
            throw new BadHttpRequestException($"Character record {character.Tid} not found", 404);
        }

        return category switch
        {
            CharacterSkillCategory.Ultimate => charRecord.UltiSkillId,
            CharacterSkillCategory.Skill1 => charRecord.Skill1Id,
            CharacterSkillCategory.Skill2 => charRecord.Skill2Id,
            _ => throw new BadHttpRequestException("Invalid skill category", 400)
        };
    }

    private static int GetSkillLevel(CharacterModel character, CharacterSkillCategory category)
    {
        return category switch
        {
            CharacterSkillCategory.Ultimate => character.UltimateLevel,
            CharacterSkillCategory.Skill1 => character.Skill1Lvl,
            CharacterSkillCategory.Skill2 => character.Skill2Lvl,
            _ => throw new BadHttpRequestException("Invalid skill category", 400)
        };
    }

    private static void SetSkillLevel(CharacterModel character, CharacterSkillCategory category, int targetLevel)
    {
        switch (category)
        {
            case CharacterSkillCategory.Ultimate:
                character.UltimateLevel = targetLevel;
                break;
            case CharacterSkillCategory.Skill1:
                character.Skill1Lvl = targetLevel;
                break;
            case CharacterSkillCategory.Skill2:
                character.Skill2Lvl = targetLevel;
                break;
            default:
                throw new BadHttpRequestException("Invalid skill category", 400);
        }
    }

    private static void AddCosts(CostRecord costRecord, Dictionary<int, int> itemCosts, Dictionary<CurrencyType, long> currencyCosts)
    {
        foreach (CostData cost in costRecord.Costs.Where(cost => cost.ItemType != RewardType.None && cost.ItemValue > 0))
        {
            if (cost.ItemType == RewardType.Currency)
            {
                CurrencyType type = (CurrencyType)cost.ItemId;
                if (!currencyCosts.TryAdd(type, cost.ItemValue))
                {
                    currencyCosts[type] += cost.ItemValue;
                }
            }
            else if (cost.ItemType == RewardType.Item)
            {
                if (!itemCosts.TryAdd(cost.ItemId, cost.ItemValue))
                {
                    itemCosts[cost.ItemId] += cost.ItemValue;
                }
            }
            else
            {
                throw new BadHttpRequestException($"Unsupported skill level cost type {cost.ItemType}", 400);
            }
        }
    }

    private static void EnsureEnoughResources(User user, Dictionary<int, int> itemCosts, Dictionary<CurrencyType, long> currencyCosts)
    {
        foreach ((CurrencyType type, long value) in currencyCosts)
        {
            if (!user.CanSubtractCurrency(type, value))
            {
                throw new BadHttpRequestException($"Insufficient currency {type}. Required: {value}, Available: {user.GetCurrencyVal(type)}", 400);
            }
        }

        foreach ((int tid, int count) in itemCosts)
        {
            DbItemData? item = user.Items.FirstOrDefault(item => item.ItemType == tid);
            if (item == null || item.Count < count)
            {
                throw new BadHttpRequestException($"Insufficient item {tid}. Required: {count}, Available: {item?.Count ?? 0}", 400);
            }
        }
    }

    private static void DeductResources(User user, Dictionary<int, int> itemCosts, Dictionary<CurrencyType, long> currencyCosts, SkillLevelUpResult result)
    {
        foreach ((CurrencyType type, long value) in currencyCosts)
        {
            user.SubtractCurrency(type, value);
            result.Currencies.Add(new NetUserCurrencyData
            {
                Type = (int)type,
                Value = user.GetCurrencyVal(type)
            });
        }

        foreach ((int tid, int count) in itemCosts)
        {
            DbItemData item = user.Items.First(item => item.ItemType == tid);
            item.Count -= count;
            result.Items.Add(NetUtils.ToNet(item));
        }
    }

    private static NetUserCharacterDefaultData ToNetCharacter(CharacterModel character)
    {
        return new()
        {
            CostumeId = character.CostumeId,
            Csn = character.Csn,
            Lv = character.Level,
            Grade = character.Grade,
            Tid = character.Tid,
            DispatchTid = character.Tid,
            Skill1Lv = character.Skill1Lvl,
            Skill2Lv = character.Skill2Lvl,
            UltiSkillLv = character.UltimateLevel
        };
    }
}
