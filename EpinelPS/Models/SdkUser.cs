
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.Models;

public class SdkUser
{
    public ulong ID { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string PlayerName { get; set; }
    public long RegisterTime { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime LastLogin { get; set;}
}
