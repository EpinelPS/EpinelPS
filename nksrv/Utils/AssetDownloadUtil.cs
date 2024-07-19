using DnsClient;
using Swan.Logging;
using System.Net;

namespace nksrv.Utils
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
                Logger.Error($"ERROR: Directory name cannot be null for request " + url + ", file path is " + targetFile);
                return null;
            }
            Directory.CreateDirectory(targetDir);

            if (!File.Exists(targetFile))
            {
                Logger.Info("Download " + targetFile);

                if (CloudIp == null)
                {
                    CloudIp = await GetCloudIpAsync();
                }

                var requestUri = new Uri("https://" + CloudIp + "/" + rawUrl);
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.TryAddWithoutValidation("host", "cloud.nikke-kr.com");
                using var response = await AssetDownloader.SendAsync(request);
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
                    Logger.Error("Failed to download " + url + " with status code " + response.StatusCode);
                    return null;
                }
            }

            return targetFile;
        }

        private static async Task<string> GetCloudIpAsync()
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync("cloud.nikke-kr.com", QueryType.A);

            var record = result.Answers.ARecords().FirstOrDefault();
            var ip = record?.Address;

            return ip.ToString();
        }
    }
}
