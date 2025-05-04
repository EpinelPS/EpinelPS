
using System.ComponentModel.DataAnnotations;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.Models.Admin;

public class RunCmdRequest
{
    [Required]
    public string cmdName { get; set; } = "";
    public string p1 { get; set; } = "";
    public string p2 { get; set; } = "";
}

public class RunCmdResponse
{
    public bool ok { get; set; }
    public string error { get; set; } = "";

    public static readonly RunCmdResponse OK = new() { ok = true };
}