using EpinelPS.LobbyServer;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;
using Newtonsoft.Json;

namespace EpinelPS.Database
{
    public class AccessToken
    {
        public string Token = "";
        public long ExpirationTime;
        public ulong UserID;
    }
    public class FieldInfo
    {
        public List<NetFieldStageData> CompletedStages = [];
        public List<NetFieldObject> CompletedObjects = [];
    }

    public class FieldInfoNew
    {
        public List<int> CompletedStages = [];
        public List<NetFieldObject> CompletedObjects = [];
    }

    public class Character
    {
        public int Csn = 0;
        public int Tid = 0;
        public int CostumeId = 0;

        public int Level = 1;
        public int UltimateLevel = 1;
        public int Skill1Lvl = 1;
        public int Skill2Lvl = 1;
        public int Grade = 0;
    }
    public class MainQuestData
    {
        public int TableId = 0;
        public bool IsReceieved = false;
    }

    public class UserPointData
    {
        public int UserLevel = 1;
        public int ExperiencePoint = 0;
    }

    public class ItemData
    {
        public int ItemType;
        public long Csn;
        public int Count;
        public int Level;
        public int Exp;
        public int Position;
        public int Corp;
        public long Isn;
    }
    public class EventData
    {
        public List<string> CompletedScenarios = new();
    }

    public class SynchroSlot
    {
        /// <summary>
        /// Index of slot, 1 based
        /// </summary>
        public int Slot;
        /// <summary>
        /// Character CSN
        /// </summary>
        public long CharacterSerialNumber;
        /// <summary>
        /// Time when slot cooldown expires
        /// </summary>
        public long AvailableAt;
    }
    public class ResetableData
    {
        public int WipeoutCount = 0;
        public bool ClearedSimulationRoom = false;
        public int InterceptionTickets = 3;
    }
    public class OutpostBuffs
    {
        public List<int> CreditPercentages = new List<int>();
        public List<int> CoreDustPercentages = new List<int>();
        public List<int> BattleDataPercentages = new List<int>();
        public List<int> UserExpPercentages = new List<int>();

        public List<int> GetPercentages(CurrencyType currency)
        {
            if (currency == CurrencyType.Gold)
                return CreditPercentages;
            else if (currency == CurrencyType.UserExp)
                return UserExpPercentages;
            else if (currency == CurrencyType.CharacterExp)
                return BattleDataPercentages;
            else if (currency == CurrencyType.CharacterExp2)
                return CoreDustPercentages;

            throw new InvalidOperationException();
        }
        public int GetTotalPercentages(CurrencyType currency)
        {
            int result = 0;
            var numbs = GetPercentages(currency);
            foreach (var item in numbs)
            {
                result += item;
            }

            return result;
        }
    }
    public class User
    {
        // User info
        public string Username = "";
        public string Password = "";
        public string PlayerName = "";
        public ulong ID;
        public long RegisterTime;
        public int LastNormalStageCleared;
        public int LastHardStageCleared;
        public string Nickname = "SomePlayer";
        public int ProfileIconId = 39900;
        public bool ProfileIconIsPrism = false;
        public int ProfileFrame = 1;
        public bool IsAdmin = false;

        public bool IsBanned = false;
        public DateTime BanStart;
        public DateTime BanEnd;
        public int BanId = 0;

        // Game data
        public List<string> CompletedScenarios = [];
        public Dictionary<string, FieldInfo> FieldInfo = []; // here for backwards compatibility

        public Dictionary<string, FieldInfoNew> FieldInfoNew = [];
        public Dictionary<string, string> MapJson = [];
        public Dictionary<CurrencyType, long> Currency = new() {
            { CurrencyType.ContentStamina, 2 }
        };
        public List<SynchroSlot> SynchroSlots = new List<SynchroSlot>();
        public bool SynchroDeviceUpgraded = false;
        public int SynchroDeviceLevel = 200;

        public ResetableData ResetableData = new();
        public List<ItemData> Items = new();
        public List<Character> Characters = [];
        public NetWholeUserTeamData RepresentationTeamData = new();
        public Dictionary<int, ClearedTutorialData> ClearedTutorialData = [];
        public NetWallpaperData[] WallpaperList = [];
        public Dictionary<int, NetUserTeamData> UserTeams = new Dictionary<int, NetUserTeamData>();
        public Dictionary<int, bool> MainQuestData = new();
        public int InfraCoreExp = 0;
        public int InfraCoreLvl = 1;
        public UserPointData userPointData = new();
        public DateTime LastLogin = DateTime.UtcNow;
        public DateTime BattleTime = DateTime.UtcNow;

        public NetOutpostBattleLevel OutpostBattleLevel = new() { Level = 1 };
        public int GachaTutorialPlayCount = 0;
        public List<int> CompletedTacticAcademyLessons = [];
        public List<int> CompletedSideStoryStages = new();

        public List<int> Memorial = new();
        public List<int> JukeboxBgm = new();

        // Event data
        public Dictionary<int, EventData> EventInfo = new();

        public OutpostBuffs OutpostBuffs = new();

        public void SetQuest(int tid, bool recievedReward)
        {
            if (MainQuestData.ContainsKey(tid))
            {
                MainQuestData[tid] = recievedReward;
                return;
            }
            else
            {
                MainQuestData.Add(tid, recievedReward);
            }
        }

        public int GenerateUniqueItemId()
        {
            var num = Rng.RandomId();

            while (Items.Any(x => x.Isn == num))
            {
                num = Rng.RandomId();
            }

            return num;
        }
        public int GenerateUniqueCharacterId()
        {
            var num = Rng.RandomId();

            while (Characters.Any(x => x.Csn == num))
            {
                num = Rng.RandomId();
            }

            return num;
        }
        public bool IsStageCompleted(int id, bool isNorm)
        {
            foreach (var item in FieldInfoNew)
            {
                if (item.Key.Contains("hard") && isNorm) continue;
                if (item.Key.Contains("normal") && !isNorm) continue;
                if (item.Value.CompletedStages.Contains(id))
                {
                    return true;
                }
            }

            return false;
        }

        public long GetCurrencyVal(CurrencyType type)
        {
            if (Currency.ContainsKey(type))
                return Currency[type];
            else
            {
                Currency.Add(type, 0);
                return 0;
            }
        }
        public void AddCurrency(CurrencyType type, long val)
        {
            if (Currency.ContainsKey(type)) Currency[type] += val;
            else Currency.Add(type, val);
        }
        public bool SubtractCurrency(CurrencyType type, long val)
        {
            if (Currency.ContainsKey(type)) Currency[type] -= val;
            else return false;

            if (Currency[type] < 0)
            {
                Currency[type] += val;
                return false;
            }
            return true;
        }
        public bool CanSubtractCurrency(CurrencyType type, long val)
        {
            if (Currency.ContainsKey(type))
            {
                if (Currency[type] >= val) return true;
                else return false;
            }
            else
            {
                if (val == 0) return true;
                else return false;
            }
        }

        public bool HasCharacter(int c)
        {
            return Characters.Any(x => x.Tid == c);
        }

        public Character? GetCharacterBySerialNumber(long value)
        {
            return Characters.Where(x => x.Csn == value).FirstOrDefault();
        }

        internal bool GetSynchro(long csn)
        {
            return SynchroSlots.Where(x => x.CharacterSerialNumber == csn).Count() >= 1;
        }
        internal int GetCharacterLevel(int csn)
        {
            var c = GetCharacterBySerialNumber(csn);
            if (c == null) throw new Exception("failed to lookup character");

            return GetCharacterLevel(csn, c.Level);
        }
        internal int GetCharacterLevel(int csn, int characterLevel)
        {
            foreach (var item in SynchroSlots)
            {
                if (item.CharacterSerialNumber == csn)
                {
                    return GetSynchroLevel();
                }
            }
            return characterLevel;
        }
        internal int GetSynchroLevel()
        {
            if (SynchroDeviceUpgraded)
                return SynchroDeviceLevel;
            var highestLevelCharacters = Characters.OrderByDescending(x => x.Level).Take(5).ToList();


            if (highestLevelCharacters.Count > 0)
            {
                return highestLevelCharacters.Last().Level;
            }
            else
            {
                return 1;
            }
        }
    }
    public class CoreInfo
    {
        public int DbVersion = 3;
        public List<User> Users = [];

        public List<AccessToken> LauncherAccessTokens = [];

        public Dictionary<string, GameClientInfo> GameClientTokens = [];
        public string ServerName = "<color=\"green\">Private Server</color>";
    }
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

            var j = JsonConvert.DeserializeObject<CoreInfo>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/db.json"));
            if (j != null)
            {
                Instance = j;

                if (Instance.DbVersion == 0)
                {
                    Instance.DbVersion = 1;
                    // In older versions, field info key used chapter number, but now difficultly is appened.
                    Console.WriteLine("Starting database update...");

                    foreach (var user in Instance.Users)
                    {
                        foreach (var f in user.FieldInfoNew.ToList())
                        {
                            var isNumeric = int.TryParse(f.Key, out int n);
                            if (isNumeric)
                            {
                                var val = f.Value;
                                user.FieldInfoNew.Remove(f.Key);
                                user.FieldInfoNew.Add(n + "_Normal", val);
                            }
                        }
                    }
                    Console.WriteLine("Database update completed");
                }
                else if (Instance.DbVersion == 1)
                {
                    Console.WriteLine("Starting database update...");
                    // there was a bug where equipment position was not saved, so remove all items from each characters
                    Instance.DbVersion = 2;
                    foreach (var user in Instance.Users)
                    {
                        foreach (var f in user.Items.ToList())
                        {
                            f.Csn = 0;
                        }
                    }
                    Console.WriteLine("Database update completed");
                }
                else if (Instance.DbVersion == 2)
                {
                    Console.WriteLine("Starting database update...");
                    // I used to use a class for FieldInfo cleared stages, but now int list is used
                    Instance.DbVersion = 3;
                    foreach (var user in Instance.Users)
                    {
                        foreach (var f in user.FieldInfo)
                        {
                            var newField = new FieldInfoNew();
                            foreach (var stage in f.Value.CompletedStages)
                            {
                                newField.CompletedStages.Add(stage.StageId);
                            }
                            user.FieldInfoNew.Add(f.Key, newField);
                        }
                        user.FieldInfo.Clear();
                    }
                    Console.WriteLine("Database update completed");
                }
                Save();

                ValidateDb();
                Console.WriteLine("Loaded db");
            }
            else
            {
                throw new Exception("Failed to read configuration json file");
            }
        }

        private static void ValidateDb()
        {
            // check if character level is valid
            foreach (var item in Instance.Users)
            {
                foreach (var c in item.Characters)
                {
                    if (c.Level > 1000)
                    {
                        Console.WriteLine($"Warning: Character level for character {c.Tid} cannot be above 1000, setting to 1000");
                        c.Level = 1000;
                    }
                }
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
