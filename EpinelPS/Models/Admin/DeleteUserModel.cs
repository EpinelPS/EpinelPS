using EpinelPS.Data;

namespace EpinelPS.Models.Admin;

public class DeleteUserModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public ulong ID { get; set; }
    public bool IsAdmin { get; set; }
    public string PlayerName { get; set; }
    public string Nickname { get; set; }
    public bool IsBanned { get; set; }
}
