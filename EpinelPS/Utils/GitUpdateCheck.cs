using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace EpinelPS.Utils;

public class GitUpdateCheck
{
    public static string GitCommit;

    static GitUpdateCheck()
    {
        GitCommit = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "<Unknown>";
        int index = GitCommit.IndexOf("+");
        if (index != -1) GitCommit = GitCommit.Substring(index + 1);
        else GitCommit = "<Unknown>";
    }

    public static async Task CheckForUpdates()
    {
        string extractPath = AppDomain.CurrentDomain.BaseDirectory + "Server_Update/";
        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
        if (Debugger.IsAttached) return;

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "EpinelPS");
        var response = await client.GetAsync("https://api.github.com/repos/EpinelPS/EpinelPS/actions/workflows/dotnet-desktop.yml/runs?branch=main&status=success&per_page=1");

        if (response.IsSuccessStatusCode)
        {
            var jsonText = await response.Content.ReadAsStringAsync();
            GithubActionsResponse? json = JsonSerializer.Deserialize<GithubActionsResponse>(jsonText);
            if (json != null && json.workflow_runs.Count > 0)
            {
                var run = json.workflow_runs[0];

                if (GitCommit != run.head_commit.id)
                {
                    Console.WriteLine("An update is available, would you like to install it?");
                    Console.WriteLine("Current Git commit: " + GitCommit);
                    Console.WriteLine("Latest Git commit: " + run.head_commit.id);
                    Console.Write("Continue (Y/N)? ");

                    var line = Console.ReadLine();
                    if (line.ToLowerInvariant() == "y")
                    {
                        Console.WriteLine("Downloading...");

                        if (OperatingSystem.IsWindows())
                            response = await client.GetAsync("https://nightly.link/EpinelPS/EpinelPS/workflows/dotnet-desktop/main/Server%20and%20Server%20selector.zip");
                        else if (OperatingSystem.IsLinux())
                            response = await client.GetAsync("https://nightly.link/EpinelPS/EpinelPS/workflows/dotnet-desktop/main/EpinelPS_linux_x64.zip");
                        else
                        {
                            Console.WriteLine("Unsupported platform");
                            return;
                        }

                        ProgressBar progress = new();

                        var filePath = AppDomain.CurrentDomain.BaseDirectory + "Server.zip";
                        using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var contentLength = response.Content.Headers.ContentLength;

                            using (var download = await response.Content.ReadAsStreamAsync())
                            {
                                // Ignore progress reporting when no progress reporter was 
                                // passed or when the content length is unknown
                                if (progress == null || !contentLength.HasValue)
                                {
                                    await download.CopyToAsync(file);
                                    return;
                                }

                                var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                                await download.CopyToAsync(file, 81920, relativeProgress);
                                progress.Report(1);
                            }
                        }

                        progress.Dispose();
                        Console.WriteLine("Wrote file to " + filePath);

                        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

                        Console.WriteLine("Extracting...");
                        ZipFile.ExtractToDirectory(filePath, extractPath);
                        File.Delete(filePath);

                        if (OperatingSystem.IsWindows())
                        {
                            Console.WriteLine("Writing... EpinelPS will be restarted automatically.");
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/c timeout /t 2 && xcopy \"{extractPath}\" \"{AppDomain.CurrentDomain.BaseDirectory}\" /Y /E && start \"\" \"{Environment.ProcessPath}\"",
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            });
                            Environment.Exit(0);
                        }
                        else if (OperatingSystem.IsLinux())
                        {
                            Console.WriteLine("Writing... Please launch EpinelPS after a few seconds");

                            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            var processPath = Environment.ProcessPath!;

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "/bin/sh",
                                Arguments =
                                    $"-c \"sleep 2; cp -a '{extractPath.TrimEnd('/')}/.' '{appDirectory}';\"",
                                UseShellExecute = false
                            });

                            Environment.Exit(0);
                        }
                    }
                }
            }
        }
        else
        {
            Logging.Warn("Failed to check for updates");
        }
    }

    public class GithubActionsResponseWorkflow
    {
        public string artifacts_url { get; set; } = "";

        public GithubActionsResponseHeadCommit head_commit { get; set; } = new();
    }
    public class GithubActionsResponseHeadCommit
    {
        public string id { get; set; } = "";
    }

    private class GithubActionsResponse
    {
        public List<GithubActionsResponseWorkflow> workflow_runs { get; set; } = [];
    }
}
/// <summary>
/// Copied from https://stackoverflow.com/a/46497896
/// </summary>
public static class StreamExtensions
{
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}
