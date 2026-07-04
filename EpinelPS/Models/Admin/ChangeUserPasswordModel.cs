using EpinelPS.Data;

namespace EpinelPS.Models.Admin;

public class ChangeUserPasswordModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public ulong ID { get; set; }
}
