using DnsClient;
using System.Net;

namespace EpinelPS.Utils;

public class AssetDownloadUtil
{
    public static readonly HttpClient AssetDownloader = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.All });

    private static string? CloudIp;
    public static async Task<string?> DownloadOrGetFileAsync(string url, CancellationToken cancellationToken)
    {
        string rawUrl = url.Replace("https://cloud.nikke-kr.com/", "").Replace("https://global-lobby.nikke-kr.com/", "").TrimStart('/');
        string targetFile = Program.GetCachePathForPath(rawUrl);
        string? targetDir = Path.GetDirectoryName(targetFile);
        if (targetDir == null)
        {
            Console.WriteLine($"ERROR: Directory name cannot be null for request " + url + ", file path is " + targetFile);
            return null;
        }
        Directory.CreateDirectory(targetDir);

        Logging.WriteLine("Game is requesting " + targetFile);
        if (!File.Exists(targetFile))
        {
            CloudIp ??= await GetIpAsync("cloud.nikke-kr.com");

            Uri requestUri = new("https://" + CloudIp + "/" + rawUrl);
            using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
            request.Headers.TryAddWithoutValidation("host", "cloud.nikke-kr.com");
            using HttpResponseMessage response = await AssetDownloader.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (!File.Exists(targetFile))
                {
                    using FileStream fss = new(targetFile, FileMode.CreateNew);
                    await response.Content.CopyToAsync(fss, cancellationToken);

                    fss.Close();
                }
            }
            else
            {
                bool fallbackSuccess = false;
                if (response.StatusCode == HttpStatusCode.NotFound && rawUrl.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    string withoutExt = rawUrl[..^4];
                    string[] variations = [$"{withoutExt}_en.mp4", $"{withoutExt}_ja.mp4", $"{withoutExt}_ko.mp4"];
                    foreach (string variation in variations)
                    {
                        Uri candidateUri = new("https://" + CloudIp + "/" + variation);
                        using HttpRequestMessage candidateRequest = new(HttpMethod.Get, candidateUri);
                        candidateRequest.Headers.TryAddWithoutValidation("host", "cloud.nikke-kr.com");
                        using HttpResponseMessage candidateResponse = await AssetDownloader.SendAsync(candidateRequest, cancellationToken);
                        if (candidateResponse.StatusCode == HttpStatusCode.OK)
                        {
                            if (!File.Exists(targetFile))
                            {
                                using FileStream fss = new(targetFile, FileMode.CreateNew);
                                await candidateResponse.Content.CopyToAsync(fss, cancellationToken);

                                fss.Close();
                            }

                            Logging.WriteLine($"Successfully downloaded fallback video candidate {variation} for {url}", LogType.Info);
                            fallbackSuccess = true;
                            break;
                        }
                    }
                }

                if (!fallbackSuccess)
                {
                    Console.WriteLine("Failed to download " + url + " with status code " + response.StatusCode);
                    return null;
                }
            }
        }

        return targetFile;
    }

    public static async Task HandleReq(HttpContext context, string all)
    {
        string? targetFile = await DownloadOrGetFileAsync(context.Request.Path.Value ?? "", CancellationToken.None);

        if (targetFile != null)
        {
            string? contentType = null;
            if (targetFile.EndsWith("mp4", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "video/mp4";
                targetFile = await CutsceneOptimizer.OptimizeVideoAsync(targetFile, context.RequestAborted);
            }
            await Results.Stream(new FileStream(targetFile, FileMode.Open, FileAccess.Read, FileShare.Read), contentType: contentType, enableRangeProcessing: true).ExecuteAsync(context);
        }
        else
            context.Response.StatusCode = 404;
    }

    public static async Task<string> GetIpAsync(string query)
    {
        LookupClient lookup = new();
        IDnsQueryResponse result = await lookup.QueryAsync(query, QueryType.A);

        DnsClient.Protocol.ARecord? record = result.Answers.ARecords().FirstOrDefault();
        IPAddress ip = record?.Address ?? throw new Exception($"Failed to find IP address of {query}, check your internet connection.");

        return ip.ToString();
    }
}
