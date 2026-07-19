using EpinelPS.Commands.Core;
using EpinelPS.Models.Admin;

namespace EpinelPS.Utils;

public static class HandleResultExtensions
{
    public static RunCmdResponse ToRunCmdResponse(this HandleResult result)
    {
        return new RunCmdResponse
        {
            ok = result.Result,
            error = result.Message ?? (result.Result ? "" : "Unknown error")
        };
    }
}
