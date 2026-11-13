using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.Controllers;

/// <summary>
/// Stub endpoints for Tencent / Level Infinite Data Subject Rights (DSR) and Account Deletion SDK queries.
/// Responds with active account status (del_account_status: 0) to avoid client retries and startup stalls.
/// </summary>
[ApiController]
public class DsrController : ControllerBase
{
    private static ContentResult BuildDsrResponse(string? seq)
    {
        string s = seq ?? "0";
        return new ContentResult
        {
            Content = "{\"ret\":0,\"msg\":\"success\",\"del_account_status\":0,\"status\":0,\"cancel_status\":0,\"seq\":\"" + s + "\",\"del_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0,\\\"seq\\\":\\\"" + s + "\\\"}\",\"data\":{\"del_account_status\":0,\"status\":0}}",
            ContentType = "application/json",
            StatusCode = 200
        };
    }

    [HttpPost("data/v1/dsr/{**action}")]
    [HttpGet("data/v1/dsr/{**action}")]
    public IActionResult DataV1Dsr(string? action, [FromQuery] string? seq) => BuildDsrResponse(seq);

    [HttpPost("delete_account/{**action}")]
    [HttpGet("delete_account/{**action}")]
    public IActionResult DeleteAccountRoot(string? action, [FromQuery] string? seq) => BuildDsrResponse(seq);

    [HttpPost("account_delete/{**action}")]
    [HttpGet("account_delete/{**action}")]
    public IActionResult AccountDeleteRoot(string? action, [FromQuery] string? seq) => BuildDsrResponse(seq);

    [HttpPost("dsr/{**action}")]
    [HttpGet("dsr/{**action}")]
    public IActionResult DsrRoot(string? action, [FromQuery] string? seq) => BuildDsrResponse(seq);
}
