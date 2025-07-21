using EpinelPS.Database;
using EpinelPS.LobbyServer;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Data;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EpinelPS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine($"EpinelPS v{Assembly.GetExecutingAssembly().GetName().Version} - https://github.com/EpinelPS/EpinelPS/");
                Console.WriteLine("This software is licensed under the AGPL-3.0 License");
                Console.WriteLine("Targeting game version " + GameConfig.Root.TargetVersion);

                GameData.Instance.GetAllCostumes(); // force static data to be loaded

                Console.WriteLine("Initializing database");
                JsonDb.Save();

                Logging.WriteLine("Register handlers");
                LobbyHandler.Init();

                Logging.WriteLine("Starting ASP.NET core on port 443");
                new Thread(() =>
                {
                    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                    // Configure HTTPS
                    HttpsConnectionAdapterOptions httpsConnectionAdapterOptions = new()
                    {
                        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                        ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                        ServerCertificate = new X509Certificate2(File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "site.pfx")))
                    };

                    builder.WebHost.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Any, 443,
                            listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                                listenOptions.UseHttps(AppDomain.CurrentDomain.BaseDirectory + @"site.pfx", "");
                            });

                        // TODO
                        serverOptions.AllowSynchronousIO = true;
                    });


                    // Add services to the container.

                    builder.Services.AddControllersWithViews();
                    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                    builder.Services.AddEndpointsApiExplorer();
                    builder.Services.AddRouting();

                    builder.Logging.ClearProviders();
                    builder.Logging.AddColorConsoleLogger(configuration =>
                    {
                        // Replace warning value from appsettings.json of "Cyan"
                        configuration.LogLevelToColorMap[LogLevel.Warning] = ConsoleColor.Yellow;
                        // Replace warning value from appsettings.json of "Red"
                        configuration.LogLevelToColorMap[LogLevel.Error] = ConsoleColor.DarkRed;
                    });


                    WebApplication app = builder.Build();

                    app.UseDefaultFiles();
                    app.UseStaticFiles();

                    // Configure the HTTP request pipeline.
                    if (app.Environment.IsDevelopment())
                    {

                    }

                    app.UseHttpsRedirection();

                    app.UseAuthorization();
                    app.UseHttpsRedirection();
                    app.UseRouting();
                    app.MapControllerRoute(
               name: "default",
               pattern: "/admin/{controller=Admin}/{action=Dashboard}/{id?}");

                    app.MapControllers();

                    app.MapPost("/$batch", HandleBatchRequests);
                    app.MapGet("/prdenv/{**all}", AssetDownloadUtil.HandleReq);
                    app.MapGet("/PC/{**all}", AssetDownloadUtil.HandleReq);
                    app.MapGet("/media/{**all}", AssetDownloadUtil.HandleReq);
                    app.MapPost("/rqd/sync", HandleRqd);

                    // NOTE: pub prefixes shows public (production server), local is local server (does not have any effect), dev is development server, etc.
                    // It does not have any effect, except for the publisher server, which adds a watermark?

                    app.MapGet("/route/{**all}", () => @"{
          ""Config"": [
            {
              ""VersionRange"": {
                ""From"": ""{GameMinVer}"",
                ""To"": ""{GameMaxVer}"",
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
                ""From"": ""{GameMinVer}"",
                ""To"": ""{GameMaxVer}"",
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
        }".Replace("{GameMinVer}", GameConfig.Root.GameMinVer).Replace("{GameMaxVer}", GameConfig.Root.GameMaxVer));

                    app.MapGet("/", () =>
                    {
                        return $"EpinelPS v{Assembly.GetExecutingAssembly().GetName().Version} - https://github.com/EpinelPS/EpinelPS/";
                    });

                    app.Run();
                }).Start();

                CliLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void HandleRqd(HttpContext context)
        {
            
        }

        private static void CliLoop()
        {
            ulong selectedUser = 0;
            string prompt = "# ";
            while (true)
            {
                Console.Write(prompt);

                string? input = Console.ReadLine();
                if (input == null) break;
                string[] args = input.Split(' ');
                if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                {

                }
                else if (input == "?" || input == "help")
                {
                    Console.WriteLine("EpinelPS CLI");
                    Console.WriteLine("NOTICE: Admin panel is available at https://localhost/admin/");
                    Console.WriteLine();
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  help - show this help");
                    Console.WriteLine("  show users - show all users");
                    Console.WriteLine("  user (user id) - select user by id");
                    Console.WriteLine("  rmuser - delete selected user");
                    Console.WriteLine("  r - load changes to database from disk. Discards data in memory.");
                    Console.WriteLine("  exit - exit server application");
                    Console.WriteLine("  completestage (chapter num)-(stage number) - complete selected stage and get rewards (and all previous ones). Example completestage 15-1. Note that the exact stage number cleared may not be exact.");
                    Console.WriteLine("  sickpulls (requires selecting user first) allows for all characters to have equal chances of getting pulled");
                    Console.WriteLine("  SetLevel (level) - Set all characters' level (between 1 and 999 takes effect on game and server restart)");
                    Console.WriteLine("  SetSkillLevel (level) - Set all characters' skill levels between 1 and 10 (takes effect on game and server restart)");
                    Console.WriteLine("  addallcharacters - Add all missing characters to the selected user with default levels and skills (takes effect on game and server restart)");
                    Console.WriteLine("  addallmaterials (amount) - Add all materials to the selected user with default levels and skills (takes effect on game and server restart)");
                    Console.WriteLine("  finishalltutorials - finish all tutorials for the selected user (takes effect on game and server restart)");
                    Console.WriteLine("  SetCoreLevel (core level / 0-3 sets stars) - Set all characters' grades based on the input (from 0 to 11)");
                    Console.WriteLine("  AddItem (id) (amount) - Adds an item to the selected user (takes effect on game and server restart)");
                    Console.WriteLine("  AddCharacter (id) - Adds a character to the selected user (takes effect on game and server restart)");
                }
                else if (input == "show users")
                {
                    Console.WriteLine("Id,Username,Nickname");
                    foreach (User item in JsonDb.Instance.Users)
                    {
                        Console.WriteLine($"{item.ID},{item.Username},{item.Nickname}");
                    }
                }
                else if (input.StartsWith("user"))
                {
                    if (args.Length == 2)
                    {
                        if (ulong.TryParse(args[1], out ulong id))
                        {
                            // check if user id exists
                            User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == id);
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
                else if (input == "addallcharacters")
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            Models.Admin.RunCmdResponse rsp = AdminCommands.AddAllCharacters(user);
                            if (!rsp.ok) Console.WriteLine(rsp.error);
                        }
                    }
                }
                else if (input.StartsWith("addallmaterials"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            int amount = 1; // Default amount if not provided
                            if (args.Length >= 2 && int.TryParse(args[1], out int parsedAmount))
                            {
                                amount = parsedAmount;
                            }

                            Models.Admin.RunCmdResponse rsp = AdminCommands.AddAllMaterials(user, amount);
                            if (!rsp.ok) Console.WriteLine(rsp.error);
                        }
                    }
                }
                else if (input == "finishalltutorials")
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            Models.Admin.RunCmdResponse rsp = AdminCommands.FinishAllTutorials(user);
                            if (!rsp.ok) Console.WriteLine(rsp.error);
                        }
                    }
                }
                else if (input.StartsWith("SetCoreLevel"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else if (args.Length == 2 && int.TryParse(args[1], out int inputGrade) && inputGrade >= 0 && inputGrade <= 11)
                        {
                            Models.Admin.RunCmdResponse rsp = AdminCommands.SetCoreLevel(user, inputGrade);
                            if (!rsp.ok) Console.WriteLine(rsp.error);
                        }
                        else
                        {
                            Console.WriteLine("Invalid argument. Core level must be between 0 and 11.");
                        }
                    }

                    // Save the updated data
                    JsonDb.Save();

                }
                else if (input == "sickpulls")
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            // Check current value of sickpulls and toggle it
                            bool currentSickPulls = EpinelPS.Database.JsonDb.IsSickPulls(user);
                            if (currentSickPulls)
                            {
                                user.sickpulls = false;
                                Console.WriteLine("sickpulls is now set to false for user " + user.Username);
                            }
                            else
                            {
                                user.sickpulls = true;
                                Console.WriteLine("sickpulls is now set to true for user " + user.Username);
                            }

                            // Save the changes to the database
                            JsonDb.Save();
                        }
                    }
                }
                else if (input.StartsWith("SetLevel"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else if (args.Length == 2 && int.TryParse(args[1], out int level) && level >= 1 && level <= 999)
                        {
                            Models.Admin.RunCmdResponse rsp = AdminCommands.SetCharacterLevel(user, level);
                            if (!rsp.ok) Console.WriteLine(rsp.error);
                        }
                        else
                        {
                            Console.WriteLine("Invalid argument. Level must be between 1 and 999.");
                        }
                    }

                    // Save the updated data
                    JsonDb.Save();
                }
                else if (input.StartsWith("SetSkillLevel"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else if (args.Length == 2 && int.TryParse(args[1], out int skillLevel) && skillLevel >= 1 && skillLevel <= 10)
                        {
                            Models.Admin.RunCmdResponse rsp = AdminCommands.SetSkillLevel(user, skillLevel);
                            if (!rsp.ok) Console.WriteLine(rsp.error);
                        }
                        else
                        {
                            Console.WriteLine("Invalid argument. Skill level must be between 1 and 10.");
                        }
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
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            Console.Write("Are you sure you want to delete user " + user.Username + "? (y/n) ");
                            string? confirm = Console.ReadLine();
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
                        if (args.Length == 2)
                        {
                            string input2 = args[1];
                            Models.Admin.RunCmdResponse rsp = AdminCommands.CompleteStage(selectedUser, input2);
                            if (!rsp.ok) Console.WriteLine(rsp.error);
                        }
                        else
                        {
                            Console.WriteLine("Invalid argument length, must be 1");
                        }
                    }
                }
                else if (input.StartsWith("AddItem"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            if (args.Length == 3)
                            {
                                if (int.TryParse(args[1], out int itemId) && int.TryParse(args[2], out int amount))
                                {
                                    Models.Admin.RunCmdResponse rsp = AdminCommands.AddItem(user, itemId, amount);
                                    if (!rsp.ok) Console.WriteLine(rsp.error);
                                }
                                else
                                {
                                    Console.WriteLine("Invalid item ID or amount");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid argument length, must be 2");
                            }
                        }
                    }
                }
                else if (input.StartsWith("AddCharacter"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
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
                                if (int.TryParse(args[1], out int characterId))
                                {
                                    Models.Admin.RunCmdResponse rsp = AdminCommands.AddCharacter(user, characterId);
                                    if (!rsp.ok) Console.WriteLine(rsp.error);
                                }
                                else
                                {
                                    Console.WriteLine("Invalid character ID");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid argument length, must be 1");
                            }
                        }
                    }
                }
                else if (input == "exit")
                {
                    Environment.Exit(0);
                }
                else if (input == "r")
                {
                    JsonDb.Reload();
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
            }
        }
        private static string LauncherEndpoint = Encoding.UTF8.GetString(Convert.FromBase64String("L25pa2tlX2xhdW5jaGVy"));


        private static async Task HandleBatchRequests(HttpContext ctx)
        {
            PacketDecryptResponse theBytes = await PacketDecryption.DecryptOrReturnContentAsync(ctx);

            // this actually uses gzip compression, unlike other requests.

            using MemoryStream streamforparser = new(theBytes.Contents);
            StreamContent content = new(streamforparser);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", (string?)ctx.Request.Headers["Content-Type"]);

            // we have the form contents, 
            MultipartMemoryStreamProvider multipart = await content.ReadAsMultipartAsync();

            HttpClient cl = new();

            // TODO: the server returns different boundary each time, looks like a GUID
            List<byte> response = [.. Encoding.UTF8.GetBytes("--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\r\n")];

            int i = 0;
            foreach (HttpContent? item in multipart.Contents)
            {
                i++;
                response.AddRange(Encoding.UTF8.GetBytes("Content-Type: application/http\r\n"));
                response.AddRange(Encoding.UTF8.GetBytes($"Content-ID: {item.Headers.NonValidated["Content-ID"]}\r\n"));
                response.AddRange(Encoding.UTF8.GetBytes("\r\n"));

                byte[] bin = await item.ReadAsByteArrayAsync();
                try
                {
                    byte[]? res = await SendReqLocalAndReadResponseAsync(bin);

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
                        response.AddRange([.. ResponseWithBytes]);
                    }
                }
                catch (Exception ex)
                {
                    List<byte> ResponseWithBytes =
   [                   .. Encoding.UTF8.GetBytes("HTTP/1.1 500 Internal Server Error\r\n"),
                            //.. Encoding.UTF8.GetBytes($"Content-Type: application/octet-stream+protobuf\r\n"),
                            .. Encoding.UTF8.GetBytes($"Content-Length: 0\r\n"),
                            .. Encoding.UTF8.GetBytes($"\r\n"),
                        ];
                    response.AddRange([.. ResponseWithBytes]);

                    Console.WriteLine("Exception during batch request: " + ex.ToString());
                }

                // add boundary, also include http newline if there is binary content

                if (i == multipart.Contents.Count)
                    response.AddRange(Encoding.UTF8.GetBytes("\r\n--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a--\r\n"));
                else
                    response.AddRange(Encoding.UTF8.GetBytes("\r\n--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\r\n"));

            }

            byte[] responseBytes = [.. response];
            ctx.Response.ContentType = "multipart/mixed; boundary=\"f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\"";
            ctx.Response.Body.Write(responseBytes);
        }
        public static string GetCachePathForPath(string path)
        {
            return AppDomain.CurrentDomain.BaseDirectory + "cache/" + path;
        }
        private static (string key, string value) GetHeader(string line)
        {
            string[] pieces = line.Split([':'], 2);

            return (pieces[0].Trim(), pieces[1].Trim());
        }
        private static async Task<byte[]?> SendReqLocalAndReadResponseAsync(byte[] bytes)
        {
            int line = 0;
            string bodyStartStr = Encoding.UTF8.GetString(bytes);

            string method;
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
                body = [.. bytes.Skip(currentByte)];
            }

            if (!url.StartsWith("/v1/"))
            {
                throw new NotImplementedException("handler for " + url + " not implemented");
            }

            url = url.Replace("/v1", "");

            // find appropriate handler
            Logging.WriteLine("BATCH " + url, LogType.Info);

            foreach (KeyValuePair<string, LobbyMsgHandler> item in LobbyHandler.Handlers)
            {
                if (item.Key == url)
                {
                    item.Value.Reset();
                    item.Value.Contents = body;
                    await item.Value.HandleAsync(authToken);
                    return item.Value.ReturnBytes;
                }
            }

            Logging.WriteLine("Handler not found: " + url, LogType.Error);

            return null;
        }
    }
}
