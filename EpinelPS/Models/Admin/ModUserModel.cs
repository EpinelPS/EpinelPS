
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;

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