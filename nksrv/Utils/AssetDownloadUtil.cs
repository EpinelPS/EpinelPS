using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.Utils
{
    public class AssetDownloadUtil
    {
        public static readonly HttpClient AssetDownloader = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.All });
        public static async Task<string?> DownloadOrGetFileAsync(string url, CancellationToken cancellationToken)
        {
            var rawUrl = url.Replace("https://cloud.nikke-kr.com", "");
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

                // TODO: Ip might change for cloud.nikke-kr.com
                string @base = rawUrl.StartsWith("/prdenv") ? "prdenv" : "media";
                if (rawUrl.StartsWith("/PC"))
                    @base = "PC";

                var requestUri = new Uri("https://35.190.17.65/" + @base + rawUrl);
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
                    return null;
                }
            }

            return targetFile;
        }
    }
}
