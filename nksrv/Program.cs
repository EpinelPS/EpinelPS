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
using nksrv.StaticInfo;
using EmbedIO.WebApi;

namespace nksrv
{
    internal class Program
    {
        static async Task Main()
        {
            Logger.UnregisterLogger<ConsoleLogger>();
            Logger.RegisterLogger(new GreatLogger());
            Logger.Info("Initializing database");
            JsonDb.Save();

            StaticDataParser.Instance.GetAllCostumes(); // force static data to be loaded

            Logger.Info("Initialize handlers");
            LobbyHandler.Init();

            Logger.Info("Starting server");

            using var server = CreateWebServer();
            await server.RunAsync();
        }
        private static WebServer CreateWebServer()
        {
            var cert = new X509Certificate2(new X509Certificate(AppDomain.CurrentDomain.BaseDirectory + @"site.pfx"));

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
                .WithModule(new ActionModule("/PC/", HttpVerbs.Any, HandleAsset))
                .WithModule(new ActionModule("/$batch", HttpVerbs.Any, HandleBatchRequests))
                .WithStaticFolder("/nikke_launcher", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", "launcher"), true)
                .WithStaticFolder("/admin/assets/", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", "admin", "assets"), true)
                .WithModule(new ActionModule("/admin", HttpVerbs.Any, HandleAdminRequest))
                .WithWebApi("/adminapi", m => m.WithController(typeof(AdminApiController)));

            // Listen for state changes.
            //server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }

        private static async Task HandleAdminRequest(IHttpContext context)
        {
            //check if user is logged in
            if (context.Request.Cookies["token"] == null && context.Request.Url.PathAndQuery != "/api/login")
            {
                context.Redirect("/adminapi/login");
                return;
            }

            //Check if authenticated correctly
            User? currentUser = null;
            if (context.Request.Url.PathAndQuery != "/api/login")
            {
                //verify token
                foreach (var item in AdminApiController.AdminAuthTokens)
                {
                    if (item.Key == context.Request.Cookies["token"].Value)
                    {
                        currentUser = item.Value;
                    }
                }
            }
            if (currentUser == null)
            {
                context.Redirect("/adminapi/login");
                return;
            }

            if (context.Request.Url.PathAndQuery == "/admin/")
            {
                context.Redirect("/admin/dashboard");
            }
            else if (context.Request.Url.PathAndQuery == "/admin/dashboard")
            {
                await context.SendStringAsync(ProcessAdminPage("dashbrd.html", currentUser), "text/html", Encoding.Unicode);
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.SendStringAsync("404 not found", "text/html", Encoding.Unicode);
            }
        }

        private static string ProcessAdminPage(string pg, User? currentUser)
        {
            var pgContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", "admin", pg));
            var nav = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", "admin", "nav.html"));

            //navbar
            pgContent = pgContent.Replace("{{navbar}}", nav);

            return pgContent;
        }
        private static async Task HandleBatchRequests(IHttpContext ctx)
        {
            var theBytes = await PacketDecryption.DecryptOrReturnContentAsync(ctx);

            // this actually uses gzip compression, unlike other requests.

            using MemoryStream streamforparser = new(theBytes.Contents);
            StreamContent content = new(streamforparser);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", ctx.Request.Headers["Content-Type"]);

            // we have the form contents, 
            var multipart = await content.ReadAsMultipartAsync();

            HttpClient cl = new();

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

                if (res != null)
                {
                    List<byte> ResponseWithBytes =
[
    .. Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n"),
                    .. Encoding.UTF8.GetBytes($"Content-Type: application/octet-stream+protobuf\r\n"),
                    .. Encoding.UTF8.GetBytes($"Content-Length: {res.Length}\r\n"),
                    .. Encoding.UTF8.GetBytes($"\r\n"),
                    .. res,
                ];
                    response.AddRange([.. ResponseWithBytes]);
                }
                else
                {
                    List<byte> ResponseWithBytes =
[                   .. Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n"),
                    //.. Encoding.UTF8.GetBytes($"Content-Type: application/octet-stream+protobuf\r\n"),
                    .. Encoding.UTF8.GetBytes($"Content-Length: 0\r\n"),
                    .. Encoding.UTF8.GetBytes($"\r\n"),
                ];
                }

                // add boundary, also include http newline if there is binary content

                if (i == multipart.Contents.Count)
                    response.AddRange(Encoding.UTF8.GetBytes("\r\n--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a--\r\n"));
                else
                    response.AddRange(Encoding.UTF8.GetBytes("\r\n--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\r\n"));

            }

            var responseBytes = response.ToArray();
            ctx.Response.ContentType = "multipart/mixed; boundary=\"f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\"";
            ctx.Response.OutputStream.Write(responseBytes);
        }
        private static async Task HandleDataEndpoint(IHttpContext ctx)
        {
            // this endpoint does not appear to be needed, it is used for telemetry
            if (ctx.RequestedPath == "/v1/dsr/query")
            {
                await WriteJsonStringAsync(ctx, "{\"ret\":0,\"msg\":\"\",\"status\":0,\"created_at\":\"0\",\"target_destroy_at\":\"0\",\"destroyed_at\":\"0\",\"err_code\":0,\"seq\":\"1\"}");
            }
            else
            {
                ctx.Response.StatusCode = 404;
            }
        }
        public static string GetCachePathForPath(string path)
        {
            return AppDomain.CurrentDomain.BaseDirectory + "cache/" + path;
        }
        private static async Task HandleAsset(IHttpContext ctx)
        {
            string? targetFile = await AssetDownloadUtil.DownloadOrGetFileAsync(ctx.Request.RawUrl, ctx.CancellationToken);

            if (targetFile == null)
            {
                Logger.Error("Download failed: " + ctx.RequestedPath);
                ctx.Response.StatusCode = 404;
                return;
            }

            try
            {
                using var fss = new FileStream(targetFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var responseStream = ctx.OpenResponseStream();
                if (ctx.RequestedPath.EndsWith(".mp4"))
                {
                    ctx.Response.ContentType = "video/mp4";
                }
                else if (ctx.RequestedPath.EndsWith(".json"))
                {
                    ctx.Response.ContentType = "application/json";
                }
                ctx.Response.StatusCode = 200;
                //ctx.Response.ContentLength64 = fss.Length; // TODO: This causes chrome to download content very slowly

                await fss.CopyToAsync(responseStream, ctx.CancellationToken);
                fss.Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }

        }
        private static async Task HandleRouteData(IHttpContext ctx)
        {
            if (ctx.RequestedPath.Contains("/route_config.json"))
            {
                await ctx.SendStringAsync(@"{
  ""Config"": [
    {
      ""VersionRange"": {
        ""From"": ""122.8.19"",
        ""To"": ""122.8.20"",
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
        ""From"": ""121.8.19"",
        ""To"": ""122.8.20"",
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
        private static async Task WriteJsonStringAsync(IHttpContext ctx, string data)
        {
            var bt = Encoding.UTF8.GetBytes(data);
            ctx.Response.ContentEncoding = null;
            ctx.Response.ContentType = "application/json";
            ctx.Response.ContentLength64 = bt.Length;
            await ctx.Response.OutputStream.WriteAsync(bt, ctx.CancellationToken);
            await ctx.Response.OutputStream.FlushAsync();
        }
        private static (string key, string value) GetHeader(string line)
        {
            var pieces = line.Split([':'], 2);

            return (pieces[0].Trim(), pieces[1].Trim());
        }
        private static async Task<byte[]?> SendReqLocalAndReadResponseAsync(byte[] bytes)
        {
            int line = 0;
            var bodyStartStr = Encoding.UTF8.GetString(bytes);

            string method;
            string url = "";
            string httpVer;
            string authToken = "";
            List<NameValueHeaderValue> headers = [];

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
                    var (key, value) = GetHeader(item);
                    headers.Add(new NameValueHeaderValue(key, value));

                    if (key == "Authorization")
                    {
                        authToken = value.Replace("Bearer ", "");
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
                body = bytes.Skip(currentByte).ToArray();
            }

            if (!url.StartsWith("/v1/"))
            {
                throw new NotImplementedException("handler for " + url + " not implemented");
            }

            url = url.Replace("/v1", "");

            // find appropriate handler
            Logger.Info("BATCH: /v1" + url);

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
            Logger.Error("HANDLER NOT FOUND: " + url);
            return null;
        }
    }
}
