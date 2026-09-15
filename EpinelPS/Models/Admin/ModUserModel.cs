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
    public Dictionary<string, Dictionary<int, double>> OverloadOptions { get; set; } = new();

    public List<SelectListItem> GetOverloadSelectList(int? selectedId = null)
    {
        var items = new List<SelectListItem>();

        foreach (var group in OverloadOptions.OrderBy(x => x.Key
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
    public Squad SquadName { get; set; }
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