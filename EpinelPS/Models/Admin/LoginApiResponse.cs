
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.Models.Admin;

public class LoginApiResponse
{
    public string Message { get; set; } = "";
    public bool OK { get; set; }
    public string Token { get; set; } = "";
}