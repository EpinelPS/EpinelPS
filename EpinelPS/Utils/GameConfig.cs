using Newtonsoft.Json;
using System.Diagnostics;

namespace EpinelPS.Utils;

public class GameConfigRoot
{
    public StaticData StaticDataMpk { get; set; } = new();
    public string ResourceBaseURL { get; set; } = "";
    /// <summary>Resource data-pack number used to locate the CDN latest file.</summary>
    public string ResourceDataPackVersion { get; set; } = "";
    public string GameMinVer { get; set; } = "";
    public string GameMaxVer { get; set; } = "";
    /// <summary>
    /// Development-only switch that grants in-app products.
    /// </summary>
    public bool EnablePurchaseSimulation { get; set; }
    /// <summary>
    /// When true, all archive records are returned as unlocked. When omitted or false,
    /// archive unlocks are determined from the user's archive progress.
    /// </summary>
    public bool? ArchiveUnlockAll { get; set; }
    /// <summary>
    /// this is only for displaying the target version in admin console or cli
    /// </summary>
    public string TargetVersion { get; set; } = "";
    /// <summary>
    /// Solo Raid boss selection mode: "Latest" (default: newest boss in static data matching current game update), "AutoCycle" (rotates weekly across all available bosses), or "Fixed" (specified by SoloRaidFixedId).
    /// </summary>
    public string SoloRaidMode { get; set; } = "Latest";
    /// <summary>
    /// Fixed Solo Raid ID when SoloRaidMode is "Fixed".
    /// </summary>
    public int SoloRaidFixedId { get; set; } = 0;
    /// <summary>
    /// Optional custom path to ffmpeg executable for cutscene video optimization.
    /// If null or empty, automatically resolves from system PATH, WinGet packages, or local tools/ directory.
    /// </summary>
    public string? FfmpegPath { get; set; }
}

public class StaticData
{
    public string Url { get; set; } = "";
    public string Version { get; set; } = "";
    public string Salt1 { get; set; } = "";
    public string Salt2 { get; set; } = "";


    public byte[] GetSalt1Bytes()
    {
        return Convert.FromBase64String(Salt1);
    }
    public byte[] GetSalt2Bytes()
    {
        return Convert.FromBase64String(Salt2);
    }
}

public static class GameConfig
{
    private static GameConfigRoot? _root;
    public static GameConfigRoot Root
    {
        get
        {
            if (_root == null)
            {
                Load();

                if (_root == null)
                    throw new Exception("Failed to read gameconfig.json");
            }

            return _root;
        }
    }

    private static string GetConfigPath()
    {
        // write to project root if running under debugger
        return Debugger.IsAttached ?
            AppDomain.CurrentDomain.BaseDirectory + "/../../../../gameconfig.json" :
        AppDomain.CurrentDomain.BaseDirectory + "/gameconfig.json";
    }

    public static void Load()
    {
        string configPath = GetConfigPath();
        if (!File.Exists(configPath))
        {
            Console.WriteLine("Gameconfig.json is not found, the game WILL NOT work!");
            _root = new GameConfigRoot();
        }
        Console.WriteLine("Loaded game config");
        _root = JsonConvert.DeserializeObject<GameConfigRoot>(File.ReadAllText(configPath));
    }

    public static void Save()
    {
        if (Root != null)
        {
            File.WriteAllText(GetConfigPath(), JsonConvert.SerializeObject(Root, Formatting.Indented));
        }
    }
}
