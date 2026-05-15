using Newtonsoft.Json;
using System.Diagnostics;

namespace EpinelPS.Utils;

public class GameConfigRoot
{
    public StaticData StaticDataMpk { get; set; } = new();
    public string ResourceBaseURL { get; set; } = "";
    public string GameMinVer { get; set; } = "";
    public string GameMaxVer { get; set; } = "";
    /// <summary>
    /// this is only for displaying the target version in admin console or cli
    /// </summary>
    public string TargetVersion { get; set; } = "";
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

    internal static void Save()
    {
        if (Root != null)
        {
            File.WriteAllText(GetConfigPath(), JsonConvert.SerializeObject(Root, Formatting.Indented));
        }
    }
}