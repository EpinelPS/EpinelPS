using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using EpinelPS.Database;
using EpinelPS.IntlServer;
using EpinelPS.LobbyServer;
using EpinelPS.LobbyServer.Msgs.Stage;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;
using Swan.Logging;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using EmbedIO.Files;

namespace EpinelPS
{
    internal class Program
    {
        static async Task Main()
        {
            try
            {
                Logger.UnregisterLogger<ConsoleLogger>();
                Logger.RegisterLogger(new GreatLogger());
                Logger.Info("Initializing database");
                JsonDb.Save();

                GameData.Instance.GetAllCostumes(); // force static data to be loaded

                Logger.Info("Initialize handlers");
                LobbyHandler.Init();

                Logger.Info("Starting server");
                new Thread(() =>
                {
                    var server = CreateWebServer();
                    server.RunAsync();
                }).Start();

                CliLoop();
            }
            catch (Exception ex)
            {
                Logger.Error("Fatal error:");
                Logger.Error(ex.ToString());
                Logger.Error("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void CliLoop()
        {
            ulong selectedUser = 0;
            string prompt = "# ";
            while (true)
            {
                Console.Write(prompt);

                var input = Console.ReadLine();
                var args = input.Split(' ');
                if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                {

                }
                else if (input == "?" || input == "help")
                {
                    Console.WriteLine("EpinelPS CLI");
                    Console.WriteLine();
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  help - show this help");
                    Console.WriteLine("  ls /users - show all users");
                    Console.WriteLine("  cd (user id) - select user by id");
                    Console.WriteLine("  rmuser - delete selected user");
                    Console.WriteLine("  ban - ban selected user from game");
                    Console.WriteLine("  unban - unban selected user from game");
                    Console.WriteLine("  exit - exit server application");
                    Console.WriteLine("  completestage (chapter num)-(stage number) - complete selected stage and get rewards (and all previous ones). Example completestage 15-1. Note that the exact stage number cleared may not be exact.");
                }
                else if (input == "ls /users")
                {
                    Console.WriteLine("Id,Username,Nickname");
                    foreach (var item in JsonDb.Instance.Users)
                    {
                        Console.WriteLine($"{item.ID},{item.Username},{item.Nickname}");
                    }
                }
                else if (input.StartsWith("cd"))
                {
                    if (args.Length == 2)
                    {
                        if (ulong.TryParse(args[1], out ulong id))
                        {
                            // check if user id exists
                            var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == id);
                            if (user != null)
                            {
                                selectedUser = user.ID;
                                Console.WriteLine("Selected user: " + user.Username);
                                prompt = "/users/" + user.Username + "# ";
                            }
                            else
                            {
                                Console.WriteLine("User not found");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Argument #1 should be a number");
                            Console.WriteLine("Usage: chroot (user id)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Incorrect number of arguments for chroot");
                        Console.WriteLine("Usage: chroot (user id)");
                    }
                }
                else if (input.StartsWith("rmuser"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            Console.Write("Are you sure you want to delete user " + user.Username + "? (y/n) ");
                            var confirm = Console.ReadLine();
                            if (confirm == "y")
                            {
                                JsonDb.Instance.Users.Remove(user);
                                JsonDb.Save();
                                Console.WriteLine("User deleted");
                                selectedUser = 0;
                                prompt = "# ";
                            }
                            else
                            {
                                Console.WriteLine("User not deleted");
                            }
                        }
                    }
                }
                else if (input.StartsWith("completestage"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            if (args.Length == 2)
                            {
                                var input2 = args[1];
                                try
                                {
                                    var chapter = int.TryParse(input2.Split('-')[0], out int chapterNumber);
                                    var stage = int.TryParse(input2.Split('-')[1], out int stageNumber);

                                    if (chapter && stage)
                                    {
                                        for (int i = 0; i < chapterNumber + 1; i++)
                                        {
                                            var stages = GameData.Instance.GetStageIdsForChapter(i, true);
                                            int target = 1;
                                            foreach (var item in stages)
                                            {
                                                if (!user.IsStageCompleted(item, true))
                                                {
                                                    Console.WriteLine("Completing stage " + item);
                                                    ClearStage.CompleteStage(user, item, true);
                                                }

                                                if (i == chapterNumber && target == stageNumber)
                                                {
                                                    break;
                                                }

                                                target++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("chapter and stage number must be a 32 bit integer");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("exception:" + ex.ToString());
                                }
                            }
                            else
                            {
                                Console.WriteLine("invalid argument length, must be 1");
                            }
                        }
                    }
                }
                else if (input == "exit")
                {
                    Environment.Exit(0);
                }
                else if (input == "ban")
                {
                    Console.WriteLine("Not implemented");
                }
                else if (input == "unban")
                {
                    Console.WriteLine("Not implemented");
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
            }
        }
        private static string LauncherEndpoint = Encoding.UTF8.GetString(Convert.FromBase64String("L25pa2tlX2xhdW5jaGVy"));

        private static FileModule AssetModule;
        private static WebServer CreateWebServer()
        {
            var cert = new X509Certificate2(new X509Certificate(AppDomain.CurrentDomain.BaseDirectory + @"site.pfx"));

            var server = new WebServer(o => o
                    .WithUrlPrefixes("https://*:443")
                    .WithMode(HttpListenerMode.EmbedIO).WithAutoLoadCertificate().WithCertificate(cert))
                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithModule(new ActionModule("/route/", HttpVerbs.Any, HandleRouteData))
                .WithModule(new ActionModule("/v1/", HttpVerbs.Any, LobbyHandler.DispatchSingle))
                .WithModule(new ActionModule("/v2/", HttpVerbs.Any, IntlHandler.Handle))
                .WithModule(new ActionModule("/account/", HttpVerbs.Any, IntlHandler.Handle))
                .WithModule(new ActionModule("/data/", HttpVerbs.Any, HandleDataEndpoint))
                .WithModule(new ActionModule("/$batch", HttpVerbs.Any, HandleBatchRequests))
                .WithModule(new ActionModule("/api/v1/", HttpVerbs.Any, IntlHandler.Handle))
                .WithStaticFolder(LauncherEndpoint, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", "launcher"), true)
                .WithStaticFolder("/admin/assets/", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "www", "admin", "assets"), true)
                .WithModule(new ActionModule("/admin", HttpVerbs.Any, HandleAdminRequest))
                .WithWebApi("/adminapi", m => m.WithController(typeof(AdminApiController)))
                .WithModule(new ActionModule("/", HttpVerbs.Any, HandleAsset));


            FileSystemProvider fileSystemProvider = new FileSystemProvider(AppDomain.CurrentDomain.BaseDirectory + "cache/", false);
            AssetModule = new FileModule("/", fileSystemProvider);
            AssetModule.Start(CancellationToken.None);
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
            if (!ctx.Request.RawUrl.StartsWith("/PC") && !ctx.Request.RawUrl.StartsWith("/media") && !ctx.Request.RawUrl.StartsWith("/prdenv"))
            {
                ctx.Response.StatusCode = 404;
                return;
            }
            string? targetFile = await AssetDownloadUtil.DownloadOrGetFileAsync(ctx.Request.RawUrl, ctx.CancellationToken);

            if (targetFile == null)
            {
                Logger.Error("Download failed: " + ctx.RequestedPath);
                ctx.Response.StatusCode = 404;
                return;
            }

            // without this, content-type will be video/mp4; charset=utf-8 which is wrong
            ctx.Response.ContentEncoding = null;
            

            await AssetModule.HandleRequestAsync(ctx);
        }
        private static async Task HandleRouteData(IHttpContext ctx)
        {
            if (ctx.RequestedPath.Contains("/route_config.json"))
            {
                // NOTE: pub prefixes shows public (production server), local is local server (does not have any effect), dev is development server, etc.
                // It does not have any effect, except for the publisher server, which adds a watermark?
                var response = @"{
  ""Config"": [
    {
    ""VersionRange"": {
        ""From"": ""{GameMinVer}"",
        ""To"": ""{GameMaxVer}"",
        ""PackageName"": ""com.proximabeta.nikke""
      },
      ""Route"": [
        {
          ""WorldId"": 1001,
          ""Name"": ""pub:priv"",
          ""Url"": ""https://global-lobby.nikke-kr.com/"",
          ""Description"": {ServerName},
          ""Tags"": []
        }
      ]
    },
    {
      ""VersionRange"": {
        ""From"": ""{GameMinVer}"",
        ""To"": ""{GameMaxVer}"",
        ""PackageName"": ""com.gamamobi.nikke""
    },
 ""Route"": [
        {
          ""WorldId"": 1001,
          ""Name"": ""pub:priv"",
          ""Url"": ""https://global-lobby.nikke-kr.com/"",
          ""Description"": {ServerName},
          ""Tags"": []
        }
      ]
    }
  ]
}";
                response = response.Replace("{GameMinVer}", GameConfig.Root.GameMinVer);
                response = response.Replace("{GameMaxVer}", GameConfig.Root.GameMaxVer);
                response = response.Replace("{ServerName}", JsonConvert.ToString(JsonDb.Instance.ServerName));
                await ctx.SendStringAsync(response, "application/json", Encoding.Default);
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
