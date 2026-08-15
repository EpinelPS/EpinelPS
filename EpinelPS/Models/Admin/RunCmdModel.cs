
using System.ComponentModel.DataAnnotations;

namespace EpinelPS.Models.Admin;

public class RunCmdRequest
{
    [Required]
    public string cmdName { get; set; } = "";
    public string p1 { get; set; } = "";
    public string p2 { get; set; } = "";
}

public static class RunCmdRequestExtensions
{
    public static string[] ToArgs(this RunCmdRequest req)
    {
        if (string.IsNullOrEmpty(req.p2)) return [];

        // New format: space-separated (e.g. "1 2", "100 5")
        if (req.p2.Contains(' '))
            return req.p2.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Legacy compatibility: dash-separated (e.g. "1-2", "100-5")
        if (req.p2.Contains('-'))
            return req.p2.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return [req.p2];
    }
}

public class RegisterAccountReg
{
    [Required]
    public string Email { get; set; } = "";
    [Required]
    public string Password { get; set; } = "";
    [Required]
    public bool IsAdmin { get; set; }
}

public class RunCmdResponse
{
    public bool ok { get; set; }
    public string error { get; set; } = "";

    public static readonly RunCmdResponse OK = new() { ok = true };
}