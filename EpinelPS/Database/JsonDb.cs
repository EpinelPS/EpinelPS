using System.Collections.Concurrent;
using EpinelPS.Data;
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
    private static readonly object _saveLock = new();
    private static int _isDirty = 0;
    private static readonly CancellationTokenSource _cts = new();
    private static readonly Task _backgroundFlusherTask;
    private static readonly ConcurrentDictionary<ulong, User> _userCache = new();


    static JsonDb()
    {
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/db.json"))
        {
            Console.WriteLine("users: warning: configuration not found, writing default data");
            Instance = new CoreInfo();
            FlushImmediate();
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

            Logging.SetOutputLevel(Instance.LogLevel);

            ValidateDb();
            Save();
            Console.WriteLine("JsonDb: Loaded");

            SyncUserCache();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => FlushImmediate();
            _backgroundFlusherTask = Task.Run(BackgroundFlusherLoop);
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
            ValidateDb();
            Console.WriteLine("Database reload complete.");
        }
            SyncUserCache();
    }

    private static void ValidateDb()
    {
        foreach (var user in Instance.Users)
        {
            // Reset daily tribe tower turn attempts on server boot
            if (user.ResetableData == null)
            {
                user.ResetableData = new();
            }
            else
            {
                user.ResetableData.TowerCount = Enum.GetValues<CorporationTowerType>()
                    .ToDictionary(t => t, _ => 0);
            }

            // Reset interception attempts on server boot
            user.ResetableData.InterceptionTickets = Instance.MaxInterceptionCount;

            // Reset solo raid attempts on server boot
            if (user.SoloRaidData != null)
            {
                foreach (var raid in user.SoloRaidData.Values)
                {
                    raid.RaidOpenCount = 0;
                    raid.TrialCount = 0;
                    raid.SoloRaidLevels.RemoveAll(l => l.IsOpen);
                }
            }

            // check if character level is valid
            foreach (var c in user.Characters)
            {
                if (c.Level > 1000)
                {
                    Console.WriteLine($"Warning: Character level for character {c.Tid} cannot be above 1000, setting to 1000");
                    c.Level = 1000;
                }
            }

            // Synchro Device: For upgraded devices, lock slots 1-5 to the top 5 characters
            if (user.Characters.Count >= 5)
            {
                var top5 = user.Characters.OrderByDescending(x => x.Level).Take(5).ToList();
                var top5Csns = top5.Select(x => (long)x.Csn).ToHashSet();

                if (user.SynchroDeviceUpgraded)
                {
                    while (user.SynchroSlots.Count < 5)
                    {
                        user.SynchroSlots.Add(new SynchroSlot { Slot = user.SynchroSlots.Count + 1, AvailableAt = 1 });
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        int slotNum = i + 1;
                        var s = user.SynchroSlots.FirstOrDefault(x => x.Slot == slotNum);
                        if (s != null && s.CharacterSerialNumber != top5[i].Csn)
                        {
                            s.CharacterSerialNumber = top5[i].Csn;
                            s.AvailableAt = 1;
                        }
                    }

                    // Prune any top 5 standard characters if they appear in slots > 5
                    foreach (var slot in user.SynchroSlots.Where(s => s.Slot > 5))
                    {
                        if (slot.CharacterSerialNumber != 0 && top5Csns.Contains(slot.CharacterSerialNumber))
                        {
                            slot.CharacterSerialNumber = 0;
                        }
                    }

                    int minStandardLv = top5.Min(x => x.Level);
                    if (user.SynchroDeviceLevel < minStandardLv)
                    {
                        user.SynchroDeviceLevel = minStandardLv;
                    }
                }
                else
                {
                    // For non-upgraded devices, standard characters are on pedestals, not slots
                    foreach (var slot in user.SynchroSlots)
                    {
                        if (slot.CharacterSerialNumber != 0 && top5Csns.Contains(slot.CharacterSerialNumber))
                        {
                            slot.CharacterSerialNumber = 0;
                        }
                    }
                }
            }

            // upgrade the gacha pull counters if using older system
            // Since we can't know what banners they pulled, we'll assume standard.
            // If user.GachaTutorialPlayCount is still 0, the user has not gone through the tutorial yet.
            try
            {
                if (user.GachaBannerMaxPulls.Count == 0 && user.GachaTutorialPlayCount > 0)
                {
                    // The old counting system would have recorded only 1 pull for the tutorial banner. The new system records 10.
                    int tutoPulls = 10;
                    int premiumPulls = Math.Max(user.GachaTutorialPlayCount - 1, 0);

                    // Fix tutorial
                    var tutorialID = 3;

                    if (user.GachaBannerMaxPulls.ContainsKey(tutorialID))
                        user.GachaBannerMaxPulls[tutorialID] = user.GachaBannerMaxPulls[tutorialID] + tutoPulls;
                    else
                        user.GachaBannerMaxPulls.Add(tutorialID, tutoPulls);

                    // Fix premium pulls
                    var premiumID = 1;

                    if (user.GachaBannerMaxPulls.ContainsKey(premiumID))
                        user.GachaBannerMaxPulls[premiumID] = user.GachaBannerMaxPulls[premiumID] + premiumPulls;
                    else
                        user.GachaBannerMaxPulls.Add(premiumID, premiumPulls);

                }
            }
            catch
            {
                Console.WriteLine($"Warning: Could not upgrade the gacha counters for user ID {user.ID}");
            }

        }
    }

    public static void SyncUserCache()
    {
        _userCache.Clear();
        if (Instance?.Users != null)
        {
            foreach (var user in Instance.Users)
            {
                _userCache[user.ID] = user;
            }
        }
    }

    public static User? GetUser(ulong id)
    {
        if (_userCache.TryGetValue(id, out var user))
            return user;

        user = Instance.Users.FirstOrDefault(x => x.ID == id);
        if (user != null)
        {
            _userCache[id] = user;
        }
        return user;
    }

    public static RankData GetRank()
    {
        return Instance.RankDatas;
    }

    public static void Save()
    {
        Interlocked.Exchange(ref _isDirty, 1);
    }

    public static void FlushImmediate()
    {
        lock (_saveLock)
        {
            Interlocked.Exchange(ref _isDirty, 0);
            FlushToFile();
        }
    }

    private static async Task BackgroundFlusherLoop()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(500, _cts.Token);
                if (Interlocked.CompareExchange(ref _isDirty, 0, 1) == 1)
                {
                    FlushToFile();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"[JsonDb] Background flush error: {ex.Message}", LogType.Error);
            }
        }
    }

    private static void FlushToFile()
    {
        lock (_saveLock)
        {
            if (Instance == null) return;
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string targetPath = Path.Combine(basePath, "db.json");
                string tempPath = Path.Combine(basePath, "db.json.tmp");
                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536))
                using (var sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                using (var jw = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(jw, Instance);
                }
                File.Move(tempPath, targetPath, overwrite: true);
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"[JsonDb] Failed to write db.json: {ex.Message}", LogType.Error);
            }
        }
    }
    public static int CurrentJukeboxBgm(int position)
    {
        var firstUser = Instance?.Users?.FirstOrDefault();
        if (firstUser?.JukeboxBgm != null && firstUser.JukeboxBgm.Count >= position && position > 0)
        {
            return firstUser.JukeboxBgm[position - 1];
        }
        return position == 2 ? 5 : 2;
    }

    public static bool IsSickPulls(User selectedUser)
    {
        if (selectedUser != null)
        {
            return selectedUser.sickpulls;
        }
        else
        {
            throw new Exception($"User not found");
        }
    }
}