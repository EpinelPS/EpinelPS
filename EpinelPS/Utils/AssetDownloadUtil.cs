using DnsClient;
using System.Net;

namespace EpinelPS.Utils
{
    public class AssetDownloadUtil
    {
        public static readonly HttpClient AssetDownloader = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.All });

        private static string? CloudIp = null;
        public static async Task<string?> DownloadOrGetFileAsync(string url, CancellationToken cancellationToken)
        {
            var rawUrl = url.Replace("https://cloud.nikke-kr.com/", "");
            string targetFile = Program.GetCachePathForPath(rawUrl);
            var targetDir = Path.GetDirectoryName(targetFile);
            if (targetDir == null)
            {
                Console.WriteLine($"ERROR: Directory name cannot be null for request " + url + ", file path is " + targetFile);
                return null;
            }
            Directory.CreateDirectory(targetDir);

            if (!File.Exists(targetFile))
            {
                Console.WriteLine("Download " + targetFile);

                if (CloudIp == null)
                {
                    CloudIp = await GetIpAsync("cloud.nikke-kr.com");
                }

                var requestUri = new Uri("https://" + CloudIp + "/" + rawUrl);
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.TryAddWithoutValidation("host", "cloud.nikke-kr.com");
                using var response = await AssetDownloader.SendAsync(request, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (!File.Exists(targetFile))
                    {
                        using var fss = new FileStream(targetFile, FileMode.CreateNew);
                        await response.Content.CopyToAsync(fss, cancellationToken);

                        fss.Close();
                    }
                }
                else
                {
                    Console.WriteLine("Failed to download " + url + " with status code " + response.StatusCode);
                    return null;
                }
            }

            return targetFile;
        }

        public static async Task HandleReq(HttpContext context)
        {
            string? targetFile = await DownloadOrGetFileAsync(context.Request.Path.Value ?? "", CancellationToken.None);

            if (targetFile != null)
            {
                string? contentType = null;
                if (targetFile.EndsWith("mp4"))
                    contentType = "video/mp4";

                await Results.Stream(new FileStream(targetFile, FileMode.Open, FileAccess.Read, FileShare.Read), contentType: contentType, enableRangeProcessing: true).ExecuteAsync(context);
            }
            else
                context.Response.StatusCode = 404;
        }

        public static async Task<string> GetIpAsync(string query)
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(query, QueryType.A);

            var record = result.Answers.ARecords().FirstOrDefault();
            var ip = record?.Address ?? throw new Exception($"Failed to find IP address of {query}, check your internet connection.");

            return ip.ToString();
        }
    }
}
