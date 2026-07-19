using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Interfaces;
using EpinelPS.LobbyServer;
using EpinelPS.Networking;
using EpinelPS.Services;
using EpinelPS.Utils;
using log4net.Config;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EpinelPS;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            Console.WriteLine($"EpinelPS v{Assembly.GetExecutingAssembly().GetName().Version} - https://github.com/EpinelPS/EpinelPS/");
            Console.WriteLine("This software is licensed under the AGPL-3.0 License");
            Console.WriteLine("Targeting game version " + GameConfig.Root.TargetVersion);

            await GameData.CreateAsync();

            Console.WriteLine("Initializing database");
            JsonDb.Save();

            Logging.WriteLine("Register handlers");
            LobbyHandler.Init();

            Logging.WriteLine("Starting ASP.NET core on port 443");
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
                serverOptions.Listen(IPAddress.Loopback, 80,
               listenOptions =>
               {
                   listenOptions.Protocols = HttpProtocols.Http1;
               });
                // TODO
                serverOptions.AllowSynchronousIO = true;
            });


            // Add services to the container.
            string connectionString = builder.Configuration.GetConnectionString("EpinelPSConnection").Replace("(startupDirectory)", AppDomain.CurrentDomain.BaseDirectory);
            string connectionType = builder.Configuration.GetConnectionString("EpinelPSConnectionType").ToLower();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddDbContext<GameContext>(options =>
            {
                switch (connectionType?.ToLowerInvariant())
                {
                    case "sql":
                        options.UseSqlServer(connectionString);
                        break;

                    case "mysql":
                        options.UseMySQL(connectionString);
                        break;

                    case "npgsql":
                        options.UseNpgsql(connectionString);
                        break;

                    default:
                        options.UseSqlite(connectionString);
                        break;
                }
            });
            builder.Services.AddControllersWithViews(options =>
            {
                options.AllowEmptyInputInBodyModelBinding = true;
                options.OutputFormatters.Insert(0, new ProtobufOutputFormatter());
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddRouting();
            builder.Services.AddHttpClient();

            builder.Logging.ClearProviders();
            builder.Logging.AddColorConsoleLogger(configuration =>
            {
                // Replace warning value from appsettings.json of "Cyan"
                configuration.LogLevelToColorMap[LogLevel.Warning] = ConsoleColor.Yellow;
                // Replace warning value from appsettings.json of "Red"
                configuration.LogLevelToColorMap[LogLevel.Error] = ConsoleColor.DarkRed;
            });


            WebApplication app = builder.Build();
            CreateDbIfNotExists(app);
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMiddleware<EncryptionMiddleware>();


            // app.UseHttpsRedirection();

            app.UseAuthorization();
            //app.UseHttpsRedirection();
            app.UseRouting();
            app.MapControllerRoute(
       name: "default",
       pattern: "/admin/{controller=Admin}/{action=Dashboard}/{id?}");

            app.MapControllers();

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

            new Thread(Commands.Services.CliLoop.Start).Start();
            app.Run();
        }
        catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design") // see https://github.com/dotnet/efcore/issues/29923
        {
            Console.WriteLine("Fatal error:");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }

    private static void CreateDbIfNotExists(IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<GameContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }
    }

    private static void HandleRqd(HttpContext context)
    {

    }

    private static readonly string LauncherEndpoint = Encoding.UTF8.GetString(Convert.FromBase64String("L25pa2tlX2xhdW5jaGVy"));

    public static string GetCachePathForPath(string path)
    {
        return AppDomain.CurrentDomain.BaseDirectory + "cache/" + path;
    }

}
