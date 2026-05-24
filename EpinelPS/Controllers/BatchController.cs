using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using static System.Collections.Specialized.BitVector32;

namespace EpinelPS.Controllers;

/// <summary>
/// Implementation of the game's request batching.
/// Forwards batch requests to the same server which isn't ideal
/// </summary>
public class BatchController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BatchController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    /// <summary>
    /// Parses request body using MultipartMemoryStreamProvider, constructs
    /// an HTTP request to send back to this server, sends it, parses response headers,
    /// writes headers and response bodies to memory stream, writes back to client using
    /// MultipartResult helper
    /// </summary>
    /// <returns></returns>

    [HttpPost]
    [Consumes("multipart/mixed")]
    [Route("/$batch")]
    public async Task<IActionResult> Post()
    {
        var boundary = GetBoundary(Request.ContentType);
        using StreamContent content = new(Request.Body);

        // hack to get MultipartMemoryStreamProvider to work
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", Request.ContentType);

        MultipartMemoryStreamProvider multipart = await content.ReadAsMultipartAsync();

        var responses = new List<(string?, byte[])>();

        MultipartResult result = new();

        foreach (var item in multipart.Contents)
        {
            var ms = new MemoryStream(); // stream is cleaned up later
            await ExecutePartRequest(await item.ReadAsByteArrayAsync(), ms);

            result.Add(new MultipartContent()
            {
                ContentId = item.Headers.NonValidated["Content-ID"].ToString(),
                ContentType = "application/http",
                Stream = ms
            });
        }

        // Build multipart response
        var responseBoundary = $"{Guid.NewGuid()}";

        return result;
    }

    private string? GetBoundary(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return null;

        var boundaryMatch = Regex.Match(contentType, @"boundary=(.+)");
        return boundaryMatch.Success ? boundaryMatch.Groups[1].Value : null;
    }
    private static (string key, string value) GetHeader(string line)
    {
        string[] pieces = line.Split([':'], 2);

        return (pieces[0].Trim(), pieces[1].Trim());
    }
    private async Task ExecutePartRequest(byte[] bytes, MemoryStream responseStream)
    {
        int line = 0;
        string bodyStartStr = Encoding.UTF8.GetString(bytes);

        string method = "POST";
        string url = "";
        string httpVer;
        string authToken = "";
        List<NameValueHeaderValue> headers = [];

        int currentByte = 0;

        foreach (string item in bodyStartStr.Split("\r\n"))
        {
            if (line == 0)
            {
                string[] parts = item.Split(" ");
                method = parts[0];
                url = parts[1];
                httpVer = parts[2];
            }
            else if (item == null || string.IsNullOrEmpty(item))
            {
                currentByte += 2;
                break;
            }
            else
            {
                (string key, string value) = GetHeader(item);
                headers.Add(new NameValueHeaderValue(key, value));

                if (key == "Authorization")
                {
                    authToken = value;
                }
            }
            currentByte += 2 + item.Length;
            line++;
        }
        byte[] body;
        if (currentByte == bytes.Length)
        {
            // empty body
            body = [];
        }
        else
        {
            body = [.. bytes.Skip(currentByte)];
        }

        var client = _httpClientFactory.CreateClient("Internal");
        var baseUrl = "http://127.0.0.1";

        var request = new HttpRequestMessage(
            new HttpMethod(method),
            $"{baseUrl}{url}");
        foreach(var item in headers)
        {
            if (item.Name == "Authorization")
            {
                request.Headers.TryAddWithoutValidation(item.Name, item.Value);
            }
        }
        if (body.Length > 0)
        {
            ByteArrayContent content = new(body);
            request.Content = content;
            request.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream+protobuf");
            request.Headers.TryAddWithoutValidation("Accept", "application/octet-stream+protobuf");
        }
        var rsp = await client.SendAsync(request);

        var contentType = rsp.Content.Headers.ContentType;
        var contentLen = rsp.Content.Headers.ContentLength;

        responseStream.Write(Encoding.UTF8.GetBytes($"HTTP/1.1 {(int)rsp.StatusCode} {rsp.StatusCode}\r\n" +
            $"Content-Type: {contentType}\r\n" +
        $"Content-Length: {contentLen}\r\n\r\n"));

        await rsp.Content.CopyToAsync(responseStream);

        responseStream.Position = 0;
    }

    public class MultipartContent
    {
        public string ContentId { get; set; }
        public string ContentType { get; set; }

        public Stream Stream { get; set; }
    }

    public class MultipartResult : Collection<MultipartContent>, IActionResult
    {
        private readonly System.Net.Http.MultipartContent content;

        public MultipartResult()
        {
            content = new System.Net.Http.MultipartContent("mixed", Guid.NewGuid().ToString());
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            foreach (var item in this)
            {
                if (item.Stream != null)
                {
                    var content = new StreamContent(item.Stream);

                    if (item.ContentType != null)
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(item.ContentType);
                    }
                    if (!string.IsNullOrEmpty(item.ContentId))
                        content.Headers.TryAddWithoutValidation("Content-ID", item.ContentId);

                    this.content.Add(content);
                }
            }

            context.HttpContext.Response.ContentLength = content.Headers.ContentLength;
            context.HttpContext.Response.ContentType = content.Headers.ContentType.ToString();

            await content.CopyToAsync(context.HttpContext.Response.Body);
        }
    }
}
