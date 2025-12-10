using System;
using System.IO;

namespace ServerSelector;

public class PathUtil
{
    public bool LauncherExists { get; private set; }
    public string? LauncherBasePath { get; set; }
    public string GameBasePath { get; set; }
    public string? SystemHostsFile
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "Drivers\\etc\\hosts");
            }
            else if (OperatingSystem.IsLinux())
            {
                return "/etc/hosts";
            }
            else throw new NotImplementedException("Unsupported operating system");
        }
    }

    public string? WineHostsFile => GameBasePath + "../../wine_prefix/drive_c/windows/system32/drivers/etc/hosts";

    public string? LauncherCertificatePath
    {
        get
        {
            if (LauncherBasePath == null) return null;

            string path = Path.Combine(LauncherBasePath, "intl_service/intl_cacert.pem");

            if (!File.Exists(path))
            {
                // Older game/SDK version
                path = Path.Combine(LauncherBasePath, "intl_service/cacert.pem");
            }
            return path;
        }
    }

    public string GamePluginsDirectory => Path.Combine(GameBasePath ?? throw new InvalidOperationException("Game path not assigned"), "nikke_Data/Plugins/x86_64/");
    public string? GameCertificatePath
    {
        get
        {
            string path = Path.Combine(GamePluginsDirectory, "intl_cacert.pem");

            if (!File.Exists(path))
            {
                // Older game/SDK version
                path = Path.Combine(GamePluginsDirectory, "cacert.pem");
            }

            return path;
        }
    }

    public string? GameSodiumPath => Path.Combine(GamePluginsDirectory, "sodium.dll");
    public string? GameSodiumBackupPath => Path.Combine(GamePluginsDirectory, "sodium.dll.bak");

    /// <summary>
    /// Sets the directory where the (game name) and Launcher directories are located
    /// </summary>
    /// <param name="basePath">directory where the (game name) and Launcher directories are located</param>
    /// <returns>Return (bool, string) where if the operation is successful, true is returned. If it fails, the string contains more details.</returns>
    public (bool, string?) SetBasePath(string basePath)
    {
        GameBasePath = Path.Combine(basePath, "NIKKE", "game");
        LauncherBasePath = Path.Combine(basePath, "Launcher");

        // Various sanity checks
        if (!Directory.Exists(GameBasePath))
        {
            return (false, $"Directory \"{GameBasePath}\" does not exist");
        }

        LauncherExists = Directory.Exists(LauncherBasePath);

        if (LauncherExists)
        {
            if (!File.Exists(Path.Combine(LauncherBasePath, "nikke_launcher.exe")))
            {
                return (false, "Game path is invalid. Make sure that nikke_launcher.exe exists in the <Game Path>/launcher folder");
            }
        }

        if (!File.Exists(GameCertificatePath))
        {
            return (false, $"Path is invalid. File \"{GameCertificatePath}\" does not exist.");
        }

        return (true, null);
    }
}
