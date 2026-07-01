using EpinelPS.Data;

namespace EpinelPS.Models.Admin;

public class ModUserModel
{
    public string Username { get; set; } = "";
    public string Nickname { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool sickpulls { get; set; }
    public bool IsBanned { get; set; }
    public ulong ID { get; set; }
}

public class ModUserCurrencyModel
{
    public ulong ID { get; set; }
    public CurrencyType ToModify { get; set; }
    public long Amount { get; set; }
    public Dictionary<CurrencyType, long> Current { get; set; }
}