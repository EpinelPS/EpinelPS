using EpinelPS.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EpinelPS.Models.Admin;

public class ModUserModel
{
    public string Username { get; set; } = "";
    public string Nickname { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool sickpulls { get; set; }
    public bool IsBanned { get; set; }
    public ulong ID { get; set; }
    public List<CharacterGearModel> OverloadedGear { get; set; } = [];
    public List<ModUserCharacterModel> Characters { get; set; } = new();
    public Dictionary<string, Dictionary<int, double>> OverloadOptions { get; set; } = new();

    public List<SelectListItem> GetOverloadSelectList(int? selectedId = null)
    {
        return BuildOverloadSelectList(OverloadOptions, selectedId);
    }

    /// <summary>
    /// Builds the option list of the overload option selects.
    /// </summary>
    public static List<SelectListItem> BuildOverloadSelectList(Dictionary<string, Dictionary<int, double>> overloadOptions, int? selectedId = null)
    {
        var items = new List<SelectListItem>();

        foreach (var group in overloadOptions.OrderBy(x => x.Key
        ))
        {
            foreach (var effect in group.Value.OrderBy(x => x.Key))
            {
                items.Add(new SelectListItem
                {
                    Value = effect.Key.ToString(),
                    Text = $"{group.Key} | {effect.Key} | {effect.Value}",
                    Selected = effect.Key == selectedId
                });
            }
        }

        return items;
    }
}

public class ModUserCurrencyModel
{
    public ulong ID { get; set; }
    public CurrencyType ToModify { get; set; }
    public long Amount { get; set; }
    public Dictionary<CurrencyType, long> Current { get; set; }
}

public class CharacterGearModel
{
    public string CharacterName { get; set; }
    public int CharacterCsn { get; set; } = 0;
    public int CharacterTid { get; set; } = 0;
    public int CharacterLevel { get; set; } = 0;
    public int Skill1 { get; set; } = 1;
    public int Skill2 { get; set; } = 1;
    public int BurstSkill { get; set; } = 1;
    public string Manufacturer { get; set; } = "";
    public List<GearModel> Gears { get; set; } = new List<GearModel>();
}

public class GearModel
{
    public long GearIsn { get; set; }
    public string GearName { get; set; } = "";
    public string GearType { get; set; }
    public int GearLevel { get; set; }
    public int Option1Id { get; set; }
    public bool Option1Lock { get; set; }
    public int Option2Id { get; set; }
    public bool Option2Lock { get; set; }
    public int Option3Id { get; set; }
    public bool Option3Lock { get; set; }
}

public class GearUpdateModel
{
    public ulong CharacterId { get; set; }
    public long GearId { get; set; }
    public int Level { get; set; }
    public int Option1 { get; set; }
    public int Option2 { get; set; }
    public int Option3 { get; set; }
}

/// <summary>
/// Lightweight row of the character list shown on the modify user page.
/// </summary>
public class ModUserCharacterModel
{
    public int Csn { get; set; }
    public int Tid { get; set; }
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public int Level { get; set; }
    public bool IsSynchro { get; set; }
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int BurstSkill { get; set; }
    public int Grade { get; set; }
    public int BreakCount { get; set; }
    public string Rarity { get; set; } = "";
    public string ElementName { get; set; } = "";
    public string WeaponType { get; set; } = "";
    public string Tags => string.Join(" · ", new[] { ElementName, WeaponType }.Where(x => !string.IsNullOrEmpty(x)));
    public int GearCount { get; set; }
}

/// <summary>
/// Detail of a single character, loaded on demand into the modify page modal.
/// </summary>
public class ModUserCharacterDetailModel
{
    public ulong UserId { get; set; }
    public ModUserCharacterModel Character { get; set; } = new();
    public List<GearModel> Gears { get; set; } = new();
    public List<DbItemData> Items { get; set; } = new();
    public Dictionary<string, Dictionary<int, double>> OverloadOptions { get; set; } = new();
}