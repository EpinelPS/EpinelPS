using EmbedIO;
using EmbedIO.Actions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using nksrv.IntlServer;
using nksrv.LobbyServer;
using nksrv.Utils;
using Swan.Logging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net.Http.Formatting;
using System.Net.Security;
using Swan.Parsers;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Swan;
using Google.Api;

namespace nksrv
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Logger.UnregisterLogger<ConsoleLogger>();
            Logger.RegisterLogger(new GreatLogger());
            JsonDb.Save();
            LobbyHandler.Init();

            // Start Webserver
            using (var server = CreateWebServer())
            {
                await server.RunAsync();
            }
        }

        private static WebServer CreateWebServer()
        {
            var cert = new X509Certificate2(new X509Certificate(@"C:\Users\Misha\nkcert\site.pfx"));

            var server = new WebServer(o => o
                    .WithUrlPrefixes("https://*:443", "http://*:80")
                    .WithMode(HttpListenerMode.EmbedIO).WithAutoLoadCertificate().WithCertificate(cert))
                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithModule(new ActionModule("/route/", HttpVerbs.Any, HandleRouteData))
                .WithModule(new ActionModule("/v1/", HttpVerbs.Any, LobbyHandler.DispatchSingle))
                .WithModule(new ActionModule("/v2/", HttpVerbs.Any, IntlHandler.Handle))
                .WithModule(new ActionModule("/prdenv/", HttpVerbs.Any, HandleAsset))
                .WithModule(new ActionModule("/account/", HttpVerbs.Any, IntlHandler.Handle))
                .WithModule(new ActionModule("/data/", HttpVerbs.Any, HandleDataEndpoint))
                .WithModule(new ActionModule("/media/", HttpVerbs.Any, HandleAsset))
                .WithModule(new ActionModule("/$batch", HttpVerbs.Any, HandleBatchRequests));

            // Listen for state changes.
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }

        private static async Task HandleBatchRequests(IHttpContext ctx)
        {
            var theBytes = await PacketDecryption.DecryptOrReturnContentAsync(ctx, true);

            // this actually uses gzip compression, unlike other requests.

            using MemoryStream streamforparser = new MemoryStream(theBytes.Contents);
            var content = new StreamContent(streamforparser);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", ctx.Request.Headers["Content-Type"]);

            // we have the form contents, 
            var multipart = await content.ReadAsMultipartAsync();

            HttpClient cl = new HttpClient();

            // TODO: the server returns different boundary each time, looks like a GUID
            List<byte> response = [.. Encoding.UTF8.GetBytes("--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\r\n")];

            int i = 0;
            foreach (var item in multipart.Contents)
            {
                i++;
                response.AddRange(Encoding.UTF8.GetBytes("Content-Type: application/http\r\n"));
                response.AddRange(Encoding.UTF8.GetBytes($"Content-ID: {item.Headers.NonValidated["Content-ID"]}\r\n"));
                response.AddRange(Encoding.UTF8.GetBytes("\r\n"));

                var bin = await item.ReadAsByteArrayAsync();
                var res = await SendReqLocalAndReadResponseAsync(bin);

                List<byte> ResponseWithBytes =
                [
                    .. Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n"),
                    .. Encoding.UTF8.GetBytes($"Content-Type: application/octet-stream+protobuf\r\n"),
                    .. Encoding.UTF8.GetBytes($"Content-Length: {res.Length}\r\n"),
                    .. Encoding.UTF8.GetBytes($"\r\n"),
                    .. res,
                ];
                response.AddRange(ResponseWithBytes.ToArray());

                // add boundary, also include http newline if there is binary content

                if (i == multipart.Contents.Count)
                    response.AddRange(Encoding.UTF8.GetBytes("\r\n--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a--\r\n"));
                else
                    response.AddRange(Encoding.UTF8.GetBytes("\r\n--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\r\n"));

            }

            var responseBytes = response.ToArray();
            File.WriteAllBytes("batch-response", responseBytes);
            ctx.Response.ContentType = "multipart/mixed; boundary=\"f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\"";
            ctx.Response.OutputStream.Write(responseBytes);
        }
        private static (string key, string value) GetHeader(string line)
        {
            var pieces = line.Split([':'], 2);

            return (pieces[0].Trim(), pieces[1].Trim());
        }
        private static async Task<byte[]> SendReqLocalAndReadResponseAsync(byte[] bytes)
        {
            int line = 0;
            var bodyStartStr = Encoding.UTF8.GetString(bytes);

            string method = "";
            string url = "";
            string httpVer = "";
            string authToken = "";
            List<NameValueHeaderValue> headers = new List<NameValueHeaderValue>();

            int currentByte = 0;

            foreach (var item in bodyStartStr.Split("\r\n"))
            {
                if (line == 0)
                {
                    var parts = item.Split(" ");
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
                    var h = GetHeader(item);
                    headers.Add(new NameValueHeaderValue(h.key, h.value));

                    if (h.key == "Authorization")
                    {
                        authToken = h.value.Replace("Bearer ", "");
                    }
                }
                currentByte += (2 + item.Length);
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
                // not empty body, TODO
                File.WriteAllBytes("notemptybody", bytes);
                body = bytes.Skip(currentByte).ToArray();
            }

            if (!url.StartsWith("/v1/"))
            {
                throw new NotImplementedException("handler for " + url + " not implemented");
            }

            url = url.Replace("/v1", "");

            // find appropriate handler
            Console.WriteLine("BATCH: /v1" + url);

            foreach (var item in LobbyHandler.Handlers)
            {
                if (item.Key == url)
                {
                    item.Value.Reset();
                    item.Value.Contents = body;
                    await item.Value.HandleAsync(authToken);
                    return item.Value.ReturnBytes;
                }
            }
            Console.WriteLine("HANDLER NOT FOUND: " + url);
            throw new Exception("handler not found: " + url);
        }

        private static byte[] ReadStream(Stream stream)
        {
            byte[] data = new byte[1024];
            List<byte> allData = new List<byte>();
            do
            {
                int numBytesRead = stream.Read(data, 0, data.Length);

                if (numBytesRead == data.Length)
                {
                    allData.AddRange(data);
                }
                else if (numBytesRead > 0)
                {
                    allData.AddRange(data.Take(numBytesRead));
                }
                else if (numBytesRead == 0)
                {
                    break;
                }
            } while (true);
            return allData.ToArray();
        }

        private static async Task HandleDataEndpoint(IHttpContext ctx)
        {
            // this endpoint does not appear to be needed, it is used for telemetry
            if (ctx.RequestedPath == "/v1/dsr/query")
            {
                WriteJsonString(ctx, "{\"ret\":0,\"msg\":\"\",\"status\":0,\"created_at\":\"0\",\"target_destroy_at\":\"0\",\"destroyed_at\":\"0\",\"err_code\":0,\"seq\":\"1\"}");
            }
            else
            {
                ctx.Response.StatusCode = 404;
            }
        }

        private static HttpClient hs = new HttpClient();
        private static async Task HandleAsset(IHttpContext ctx)
        {
            string fs = AppDomain.CurrentDomain.BaseDirectory + "cache" + ctx.RequestedPath;
            Directory.CreateDirectory(Path.GetDirectoryName(fs));
            if (!File.Exists(fs))
            {
                Logger.Info("Download " + fs);

                // TODO: Ip might change
                string @base = ctx.Request.RawUrl.StartsWith("/prdenv") ? "prdenv" : "media";
                var requestUri = new Uri("https://43.132.66.200/" + @base + ctx.RequestedPath);
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.TryAddWithoutValidation("host", "cloud.nikke-kr.com");
                using var response = await hs.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var fss = new FileStream(fs, FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fss);

                        fss.Close();
                    }
                }
                else
                {
                    Logger.Error("FAILED TO DOWNLOAD FILE: " + ctx.RequestedPath);
                    ctx.Response.StatusCode = 404;
                    return;
                }
            }
            try
            {



                using (var fss = new FileStream(fs, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var responseStream = ctx.OpenResponseStream())
                    {
                        if (ctx.RequestedPath.EndsWith(".mp4"))
                        {
                            ctx.Response.ContentType = "video/mp4";
                        }
                        else if (ctx.RequestedPath.EndsWith(".json"))
                        {
                            ctx.Response.ContentType = "application/json";
                        }
                        ctx.Response.StatusCode = 200;
                        //ctx.Response.ContentLength64 = fss.Length;

                        
                        fss.CopyTo(responseStream);
                        fss.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
          
        }

        private static void WriteData<T>(IHttpContext ctx, T data, bool encrypted = false) where T : IMessage, new()
        {
            ctx.Response.ContentEncoding = null;
            ctx.Response.ContentType = "application/octet-stream+protobuf";
            ctx.Response.ContentLength64 = data.CalculateSize();
            var x = new CodedOutputStream(ctx.Response.OutputStream);
            data.WriteTo(x);
            x.Flush();
        }
        private static void WriteJsonString(IHttpContext ctx, string data)
        {
            var bt = Encoding.UTF8.GetBytes(data);
            ctx.Response.ContentEncoding = null;
            ctx.Response.ContentType = "application/json";
            ctx.Response.ContentLength64 = bt.Length;
            ctx.Response.OutputStream.Write(bt, 0, bt.Length);
            ctx.Response.OutputStream.Flush();
        }

        private static async Task HandleRouteData(IHttpContext ctx)
        {
            if (ctx.RequestedPath.Contains("/route_config.json"))
            {
                await ctx.SendStringAsync(@"{
  ""Config"": [
    {
      ""VersionRange"": {
        ""From"": ""121.8.9"",
        ""To"": ""121.10.2"",
        ""PackageName"": ""com.proximabeta.nikke""
      },
      ""Route"": [
        {
          ""WorldId"": 81,
          ""Name"": ""pub:live-jp"",
          ""Url"": ""https://jp-lobby.nikke-kr.com/"",
          ""Description"": ""JAPAN"",
          ""Tags"": []
        },
        {
          ""WorldId"": 82,
          ""Name"": ""pub:live-na"",
          ""Url"": ""https://us-lobby.nikke-kr.com/"",
          ""Description"": ""NA"",
          ""Tags"": []
        },
        {
          ""WorldId"": 83,
          ""Name"": ""pub:live-kr"",
          ""Url"": ""https://kr-lobby.nikke-kr.com/"",
          ""Description"": ""KOREA"",
          ""Tags"": []
        },
        {
          ""WorldId"": 84,
          ""Name"": ""pub:live-global"",
          ""Url"": ""https://global-lobby.nikke-kr.com/"",
          ""Description"": ""GLOBAL"",
          ""Tags"": []
        },
        {
          ""WorldId"": 85,
          ""Name"": ""pub:live-sea"",
          ""Url"": ""https://sea-lobby.nikke-kr.com/"",
          ""Description"": ""SEA"",
          ""Tags"": []
        }
      ]
    },
    {
      ""VersionRange"": {
        ""From"": ""121.8.9"",
        ""To"": ""121.10.2"",
        ""PackageName"": ""com.gamamobi.nikke""
      },
      ""Route"": [
        {
          ""WorldId"": 91,
          ""Name"": ""pub:live-hmt"",
          ""Url"": ""https://hmt-lobby.nikke-kr.com/"",
          ""Description"": ""HMT"",
          ""Tags"": []
        }
      ]
    }
  ]
}", "application/json", Encoding.Default);
            }
            else
            {
                Console.WriteLine("ROUTE - Unknown: " + ctx.RequestedPath);
                ctx.Response.StatusCode = 404;
            }
        }
    }
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Request:");
            Console.WriteLine(request.ToString());
            if (request.Content != null)
            {
                Console.WriteLine(await request.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());
            if (response.Content != null)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            Console.WriteLine();

            return response;
        }
    }
}
