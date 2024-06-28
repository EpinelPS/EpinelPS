using ASodium;
using Newtonsoft.Json;
using nksrv.LobbyServer;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;

namespace nksrv.Utils
{
    public class AccessToken
    {
        public string Token;
        public long ExpirationTime;
        public ulong UserID;
    }
    public class FieldInfo
    {
        public List<NetFieldStageData> CompletedStages = new();
    }

    public class Character
    {
        // TODO
        public int Csn = 0;
        public int Tid = 0;
        public int CostumeId = 0;

        public int Level = 1;
        public int UltimateLevel = 1;
        public int Skill1Lvl = 1;
        public int Skill2Lvl = 1;
        public int Grade = 0;
    }
    
    public class User
    {
        // User info
        public string Username = "";
        public string Password = "";
        public string PlayerName = "";
        public ulong ID;
        public long RegisterTime;
        public int LastStageCleared;
        public string Nickname = "SomePLayer";


        // Game data
        public List<string> CompletedScenarios = new();
        public Dictionary<int, FieldInfo> FieldInfo = new();
        public Dictionary<string, string> MapJson = new();
        public Dictionary<CurrencyType, long> Currency = new Dictionary<CurrencyType, long>() {
            { CurrencyType.ContentStamina, 2 },

            { CurrencyType.Gold, 1 },
              { CurrencyType.CharPremiumTicket, 23422 }
        };

        public List<Character> Characters = new();
        public NetWholeUserTeamData TeamData = new();
        public List<int> ClearedTutorials = new();
    }
    public class CoreInfo
    {
        public List<User> Users = new List<User>();

        public List<AccessToken> LauncherAccessTokens = new List<AccessToken>();


        public Dictionary<string, GameClientInfo> GameClientTokens = new Dictionary<string, GameClientInfo>();
    }
    internal class JsonDb
    {
        public static CoreInfo Instance { get; internal set; }
        public static byte[] ServerPrivateKey = Convert.FromBase64String("FSUY8Ohd942n5LWAfxn6slK3YGwc8OqmyJoJup9nNos=");
        public static byte[] ServerPublicKey = Convert.FromBase64String("04hFDd1e/BOEF2h4b0MdkX2h6W5REeqyW+0r9+eSeh0=");

        static JsonDb()
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/db.json"))
            {
                Logger.Warn("users: warning: configuration not found, writing default data");
                Instance = new CoreInfo();
                Save();
            }
            Logger.Info("Loaded db");


            var j = JsonConvert.DeserializeObject<CoreInfo>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/db.json"));
            if (j != null)
            {
                Instance = j;
                Save();
            }
            else
            {
                throw new Exception("Failed to read configuration json file");
            }
        }
        public static User? GetUser(ulong id)
        {
            return Instance.Users.Where(x => x.ID == id).FirstOrDefault();
        }
        public static void Save()
        {
            if (Instance != null)
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/db.json", JsonConvert.SerializeObject(Instance, Formatting.Indented));
            }
        }
    }
}
