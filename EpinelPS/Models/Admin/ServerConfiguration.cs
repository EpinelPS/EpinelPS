using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.Models.Admin;

public class ServerConfiguration
{
    [BindProperty]
    public LogType LogType { get; set; }
}