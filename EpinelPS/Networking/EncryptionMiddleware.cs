using EpinelPS.Utils;

namespace EpinelPS.Networking;

public class EncryptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        string path = context.Request.Path.Value ?? "";
        if (IsDeleteAccountOrDsrPath(path))
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"ret\":0,\"msg\":\"success\",\"del_account_status\":0,\"status\":0,\"cancel_status\":0,\"del_account_info\":\"{\\\"ret\\\":0,\\\"msg\\\":\\\"\\\",\\\"status\\\":0,\\\"created_at\\\":\\\"0\\\",\\\"target_destroy_at\\\":\\\"0\\\",\\\"destroyed_at\\\":\\\"0\\\",\\\"err_code\\\":0}\",\"data\":{\"del_account_status\":0,\"status\":0}}");
            return;
        }

        if (context.Request.Path.ToString().StartsWith("/v1") || context.Request.Path.ToString().StartsWith("/$batch"))
        {
            var x = await PacketDecryption.DecryptOrReturnContentAsync(context);
            
            if (x.Contents.Length > 0)
            {
                context.Request.Body = new MemoryStream(x.Contents);
            }
            context.Items["UserID"] = x.UserId;
        }

        await _next(context);
    }

    private static bool IsDeleteAccountOrDsrPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        ReadOnlySpan<char> span = path.AsSpan();
        return span.Contains("delete_account", StringComparison.OrdinalIgnoreCase)
            || span.Contains("account_delete", StringComparison.OrdinalIgnoreCase)
            || span.Contains("dsr/query", StringComparison.OrdinalIgnoreCase)
            || span.Contains("dsr/cancel", StringComparison.OrdinalIgnoreCase)
            || span.Contains("del_account", StringComparison.OrdinalIgnoreCase);
    }
}
