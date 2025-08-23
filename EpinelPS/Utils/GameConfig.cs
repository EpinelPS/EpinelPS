using EpinelPS.Database;
using Newtonsoft.Json;

namespace EpinelPS.Utils
{
    public class GameConfigRoot
    {
        public StaticData StaticData { get; set; } = new();
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
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/gameconfig.json"))
                    {
                        Console.WriteLine("Gameconfig.json is not found, the game WILL NOT work!");
                        _root = new GameConfigRoot();
                    }
                    Console.WriteLine("Loaded game config");


                    _root = JsonConvert.DeserializeObject<GameConfigRoot>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/gameconfig.json"));

                    if (_root == null)
                    {
                        throw new Exception("Failed to read gameconfig.json");
                    }
                }

                return _root;
            }
        }

        internal static void Save()
        {
            if (Root != null)
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/gameconfig.json", JsonConvert.SerializeObject(Root, Formatting.Indented));
            }
        }
    }
}