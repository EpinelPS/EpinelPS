using DnsClient;
using System.Collections.Concurrent;
using System.Net;

namespace EpinelPS.Utils;

public class AssetDownloadUtil
{
    public static readonly HttpClient AssetDownloader = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.All });
    private static readonly ConcurrentDictionary<string, Task<string?>> InFlightDownloads = new(StringComparer.OrdinalIgnoreCase);
    private static readonly LookupClient DnsLookup = new();
    private static readonly SemaphoreSlim DnsLock = new(1, 1);
    private static string? CloudIp;
    public static async Task<string?> DownloadOrGetFileAsync(string url, CancellationToken cancellationToken)
    {
        string rawUrl = url.Replace("https://cloud.nikke-kr.com/", "").Replace("https://global-lobby.nikke-kr.com/", "").TrimStart('/');
        string targetFile = Program.GetCachePathForPath(rawUrl);
        if (File.Exists(targetFile) && new FileInfo(targetFile).Length > 0)
        {
            return targetFile;
        }

        string? targetDir = Path.GetDirectoryName(targetFile);
        if (targetDir == null)
        {
            Console.WriteLine($"ERROR: Directory name cannot be null for request " + url + ", file path is " + targetFile);
            return null;
        }
        Directory.CreateDirectory(targetDir);

        return await InFlightDownloads.GetOrAdd(targetFile, _ => DownloadInternalAsync(url, rawUrl, targetFile, cancellationToken));
    }

    private static async Task<string?> DownloadInternalAsync(string url, string rawUrl, string targetFile, CancellationToken cancellationToken)
    {
        try
        {
            if (File.Exists(targetFile) && new FileInfo(targetFile).Length > 0)
            {
                return targetFile;
            }

            Logging.WriteLine("Game is requesting " + targetFile);
            string ip = await GetCloudIpAsync();

            string tempFile = targetFile + ".tmp." + Guid.NewGuid().ToString("N");
            Uri requestUri = new("https://" + ip + "/" + rawUrl);
            using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
            request.Headers.TryAddWithoutValidation("host", "cloud.nikke-kr.com");
            using HttpResponseMessage response = await AssetDownloader.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (FileStream fss = new(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fss, cancellationToken);
                }
                File.Move(tempFile, targetFile, overwrite: true);
                return targetFile;
            }

            bool fallbackSuccess = false;
            if (response.StatusCode == HttpStatusCode.NotFound && rawUrl.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                string withoutExt = rawUrl[..^4];
                string[] variations = [$"{withoutExt}_en.mp4", $"{withoutExt}_ja.mp4", $"{withoutExt}_ko.mp4"];
                foreach (string variation in variations)
                {
                    Uri candidateUri = new("https://" + ip + "/" + variation);
                    using HttpRequestMessage candidateRequest = new(HttpMethod.Get, candidateUri);
                    candidateRequest.Headers.TryAddWithoutValidation("host", "cloud.nikke-kr.com");
                    using HttpResponseMessage candidateResponse = await AssetDownloader.SendAsync(candidateRequest, cancellationToken);
                    if (candidateResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (FileStream fss = new(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await candidateResponse.Content.CopyToAsync(fss, cancellationToken);
                        }
                        File.Move(tempFile, targetFile, overwrite: true);
                        Logging.WriteLine($"Successfully downloaded fallback video candidate {variation} for {url}", LogType.Info);
                        fallbackSuccess = true;
                        return targetFile;
                    }
                }
            }

            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { }
            }

            if (!fallbackSuccess)
            {
                Console.WriteLine("Failed to download " + url + " with status code " + response.StatusCode);
                return null;
            }

            return targetFile;
        }
        finally
        {
            InFlightDownloads.TryRemove(targetFile, out _);
        }
    }

    private static async Task<string> GetCloudIpAsync()
    {
        if (CloudIp != null) return CloudIp;
        await DnsLock.WaitAsync();
        try
        {
            if (CloudIp != null) return CloudIp;
            CloudIp = await GetIpAsync("cloud.nikke-kr.com");
            return CloudIp;
        }
        finally
        {
            DnsLock.Release();
        }
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
        IDnsQueryResponse result = await DnsLookup.QueryAsync(query, QueryType.A);
        DnsClient.Protocol.ARecord? record = result.Answers.ARecords().FirstOrDefault();
        IPAddress ip = record?.Address ?? throw new Exception($"Failed to find IP address of {query}, check your internet connection.");
        return ip.ToString();
    }
}
