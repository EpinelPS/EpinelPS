using System.ComponentModel.DataAnnotations;

namespace EpinelPS.Models.Admin;

public class LoginApiBody
{
    [Required]
    public string Username { get; set; } = "";
    [Required]
    public string Password { get; set; } = "";
}