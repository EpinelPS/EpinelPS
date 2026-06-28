using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace EpinelPS.Controllers;

[ApiController]
public class JupiterCheckoutController : ControllerBase
{
    [HttpGet("/inappshop/jupiter/success")]
    public ContentResult Success([FromQuery(Name = "reference_id")] string? referenceId)
    {
        string encodedReferenceId = WebUtility.HtmlEncode(referenceId ?? string.Empty);
        string jsonReferenceId = JsonSerializer.Serialize(referenceId ?? string.Empty);
        string html = $$"""
            <!doctype html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Purchase Complete</title>
                <script>
                    const referenceId = {{jsonReferenceId}};
                    function notifyAndClose() {
                        const payload = { type: "purchaseComplete", referenceId };
                        try { window.chrome?.webview?.postMessage(payload); } catch (_) { }
                        try { window.webkit?.messageHandlers?.purchaseComplete?.postMessage(payload); } catch (_) { }
                        try { window.postMessage(payload, "*"); } catch (_) { }
                        try { window.close(); } catch (_) { }
                    }
                    window.addEventListener("load", () => setTimeout(notifyAndClose, 4000));
                </script>
            </head>
            <body>
                <main style="font-family: sans-serif; padding: 24px;">
                    <h1>Purchase Complete</h1>
                    <p>Reference: {{encodedReferenceId}}</p>
                    <p>This page will close automatically.</p>
                </main>
            </body>
            </html>
            """;

        return Content(html, "text/html");
    }
}
