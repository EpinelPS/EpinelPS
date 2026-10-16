using EpinelPS.Utils;

namespace EpinelPS.Networking;

public class EncryptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
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
}
