using EpinelPS.Utils;
using Newtonsoft.Json;
using Paseto;
using Paseto.Builder;

namespace EpinelPS.Database;

internal class JsonDb
{
    public static CoreInfo Instance { get; internal set; }

    // Note: change this in sodium
    public static byte[] ServerPrivateKey = Convert.FromBase64String("FSUY8Ohd942n5LWAfxn6slK3YGwc8OqmyJoJup9nNos=");
    public static byte[] ServerPublicKey = Convert.FromBase64String("04hFDd1e/BOEF2h4b0MdkX2h6W5REeqyW+0r9+eSeh0=");

    static JsonDb()
    {
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/db.json"))
        {
            Console.WriteLine("users: warning: configuration not found, writing default data");
            Instance = new CoreInfo();
            Save();
        }       
       

        var text = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/db.json");
        if (text.Contains("Char_Premium_Ticket"))
        {
            text = text.Replace("Char_Premium_Ticket", "CharPremiumTicket");
            text = text.Replace("Char_Customize_Ticket", "CharCustomizeTicket");
            text = text.Replace("Char_Select_01_Ticket", "CharSelect01Ticket");
            text = text.Replace("Char_Select_02_Ticket", "CharSelect02Ticket");
        }

        var j = JsonConvert.DeserializeObject<CoreInfo>(text);
        if (j != null)
        {
            Instance = j;

            if (Instance.DbVersion != 5)
            {
                Logging.Warn("!!!WARNING!!!");
                Logging.Warn("Database version is extremely out of date.");
                Logging.Warn("It is recommended to delete db.json to avoid issues.");
            }

            if (Instance.LauncherTokenKey.Length == 0)
            {
                Console.WriteLine("Launcher token key is null, generating new key");

                var pasetoKey = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                             .GenerateSymmetricKey();
                Instance.LauncherTokenKey = pasetoKey.Key.ToArray();
            }
            if (Instance.EncryptionTokenKey.Length == 0)
            {
                Console.WriteLine("EncryptionTokenKey is null, generating new key");

                var pasetoKey = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                             .GenerateSymmetricKey();
                Instance.EncryptionTokenKey = pasetoKey.Key.ToArray();
            }

            Save();

            Logging.SetOutputLevel(Instance.LogLevel);

            Console.WriteLine("JsonDb: Loaded");
        }
        else
        {
            throw new Exception("Failed to read configuration json file");
        }

    }

    public static void Reload()
    {
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/db.json"))
        {
            Console.WriteLine("users: warning: configuration not found, writing default data");
            Instance = new CoreInfo();
            Save();
        }

        var j = JsonConvert.DeserializeObject<CoreInfo>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/db.json"));
        if (j != null)
        {
            Instance = j;
            Console.WriteLine("Database reload complete.");
        }
    }

    public static RankData GetRank()
    {
        return Instance.RankDatas;
    }

    public static void Save()
    {
        if (Instance != null)
        {
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/db.json", JsonConvert.SerializeObject(Instance, Formatting.Indented));
        }
    }
}