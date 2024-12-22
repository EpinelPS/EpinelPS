using EpinelPS.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Cryptography;

namespace EpinelPS.StaticInfo
{
    public class GameData
    {
        private static GameData? _instance;
        public static GameData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = BuildAsync().Result;
                }

                return _instance;
            }
        }

        private ZipFile MainZip;
        private MemoryStream ZipStream;
        private Dictionary<int, MainQuestCompletionRecord> questDataRecords;
        private Dictionary<int, CampaignStageRecord> stageDataRecords;
        private Dictionary<int, RewardTableRecord> rewardDataRecords;
        private JArray userExpDataRecords;
        private Dictionary<int, CampaignChapterRecord> chapterCampaignData;
        private JArray characterCostumeTable;
        public Dictionary<int, CharacterRecord> characterTable;
        public Dictionary<int, ClearedTutorialData> tutorialTable;
        public Dictionary<int, ItemEquipRecord> itemEquipTable;
        public Dictionary<int, ItemMaterialRecord> itemMaterialTable;
        public Dictionary<int, ItemEquipExpRecord> itemEquipExpTable;
        public Dictionary<int, ItemEquipGradeExpRecord> ItemEquipGradeExpTable;
        private Dictionary<string, JArray> FieldMapData = new Dictionary<string, JArray>();  // Fixed initialization
        private Dictionary<int, CharacterLevelData> LevelData = new Dictionary<int, CharacterLevelData>();  // Fixed initialization
        private Dictionary<int, TacticAcademyLessonRecord> TacticAcademyLessons = new Dictionary<int, TacticAcademyLessonRecord>();  // Fixed initialization
        public Dictionary<int, int> SidestoryRewardTable = new Dictionary<int, int>();  // Fixed initialization
        public Dictionary<string, int> PositionReward = new Dictionary<string, int>();  // Fixed initialization
        public Dictionary<int, FieldItemRecord> FieldItems = new Dictionary<int, FieldItemRecord>();  // Fixed initialization
        public Dictionary<int, OutpostBattleTableRecord> OutpostBattle = new Dictionary<int, OutpostBattleTableRecord>();  // Fixed initialization
        public Dictionary<int, JukeboxListRecord> jukeboxListDataRecords;
        private Dictionary<int, JukeboxThemeRecord> jukeboxThemeDataRecords;
        public Dictionary<int, GachaType> gachaTypes = new Dictionary<int, GachaType>(); // Fixed initialization
        public Dictionary<int, EventManager> eventManagers = new Dictionary<int, EventManager>();
        public Dictionary<int, LiveWallpaperRecord> lwptablemgrs = new Dictionary<int, LiveWallpaperRecord>(); // Fixed initialization
        public Dictionary<int, AlbumResourceRecord> albumResourceRecords = new Dictionary<int, AlbumResourceRecord>();
		public Dictionary<int, UserFrameTableRecord> userFrameTable = new Dictionary<int, UserFrameTableRecord>();
		public Dictionary<int, ArchiveRecordManagerRecord> archiveRecordManagerTable = new Dictionary<int, ArchiveRecordManagerRecord>();
		public Dictionary<int, ArchiveEventStoryRecord> archiveEventStoryRecords = new Dictionary<int, ArchiveEventStoryRecord>();
		public Dictionary<int, ArchiveEventQuestRecord> archiveEventQuestRecords = new Dictionary<int, ArchiveEventQuestRecord>();
		public Dictionary<int, ArchiveEventDungeonStageRecord> archiveEventDungeonStageRecords = new Dictionary<int, ArchiveEventDungeonStageRecord>();
		public Dictionary<int, UserTitleRecord> userTitleRecords = new Dictionary<int, UserTitleRecord>();
        public Dictionary<int, ArchiveMessengerConditionRecord> archiveMessengerConditionRecords = [];
        public Dictionary<int, CharacterStatRecord> characterStatTable = [];
        public Dictionary<int, SkillInfoRecord> skillInfoTable = [];
        public Dictionary<int, CostRecord> costTable = [];
        public Dictionary<string, MidasProductRecord> mediasProductTable = [];
        public Dictionary<int, TowerRecord> towerTable = [];



        public byte[] Sha256Hash;
        public int Size;

        static async Task<GameData> BuildAsync()
        {
            await Load();

            Console.WriteLine("Preparing");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            await Instance.Parse();

            stopWatch.Stop();
            Console.WriteLine("Preparing took " + stopWatch.Elapsed);
            return Instance;
        }

        public GameData(string filePath)
        {
            if (!File.Exists(filePath)) throw new ArgumentException("Static data file must exist", nameof(filePath));

            // disable warnings
            ZipStream = new();
            questDataRecords = [];
            stageDataRecords = [];
            rewardDataRecords = [];
            userExpDataRecords = [];
            chapterCampaignData = [];
            characterCostumeTable = [];
            characterTable = [];
            tutorialTable = [];
            itemEquipTable = [];
            itemMaterialTable = [];
            itemEquipExpTable = [];
            ItemEquipGradeExpTable = [];
            characterStatTable = [];
            skillInfoTable = [];
            costTable = [];

            // Initialize Jukebox data dictionaries
            jukeboxListDataRecords = [];
            jukeboxThemeDataRecords = [];
			archiveMessengerConditionRecords = [];

            var rawBytes = File.ReadAllBytes(filePath);
            Sha256Hash = SHA256.HashData(rawBytes);
            Size = rawBytes.Length;

            LoadGameData(filePath);
            if (MainZip == null) throw new Exception("failed to read zip file");
        }

        #region Data loading
        private static byte[] PresharedValue = [0xCB, 0xC2, 0x1C, 0x6F, 0xF3, 0xF5, 0x07, 0xF5, 0x05, 0xBA, 0xCA, 0xD4, 0x98, 0x28, 0x84, 0x1F, 0xF0, 0xD1, 0x38, 0xC7, 0x61, 0xDF, 0xD6, 0xE6, 0x64, 0x9A, 0x85, 0x13, 0x3E, 0x1A, 0x6A, 0x0C, 0x68, 0x0E, 0x2B, 0xC4, 0xDF, 0x72, 0xF8, 0xC6, 0x55, 0xE4, 0x7B, 0x14, 0x36, 0x18, 0x3B, 0xA7, 0xD1, 0x20, 0x81, 0x22, 0xD1, 0xA9, 0x18, 0x84, 0x65, 0x13, 0x0B, 0xED, 0xA3, 0x00, 0xE5, 0xD9];
        private static RSAParameters LoadParameters = new RSAParameters()
        {
            Exponent = [0x01, 0x00, 0x01],
            Modulus = [0x89, 0xD6, 0x66, 0x00, 0x7D, 0xFC, 0x7D, 0xCE, 0x83, 0xA6, 0x62, 0xE3, 0x1A, 0x5E, 0x9A, 0x53, 0xC7, 0x8A, 0x27, 0xF3, 0x67, 0xC1, 0xF3, 0xD4, 0x37, 0xFE, 0x50, 0x6D, 0x38, 0x45, 0xDF, 0x7E, 0x73, 0x5C, 0xF4, 0x9D, 0x40, 0x4C, 0x8C, 0x63, 0x21, 0x97, 0xDF, 0x46, 0xFF, 0xB2, 0x0D, 0x0E, 0xDB, 0xB2, 0x72, 0xB4, 0xA8, 0x42, 0xCD, 0xEE, 0x48, 0x06, 0x74, 0x4F, 0xE9, 0x56, 0x6E, 0x9A, 0xB1, 0x60, 0x18, 0xBC, 0x86, 0x0B, 0xB6, 0x32, 0xA7, 0x51, 0x00, 0x85, 0x7B, 0xC8, 0x72, 0xCE, 0x53, 0x71, 0x3F, 0x64, 0xC2, 0x25, 0x58, 0xEF, 0xB0, 0xC9, 0x1D, 0xE3, 0xB3, 0x8E, 0xFC, 0x55, 0xCF, 0x8B, 0x02, 0xA5, 0xC8, 0x1E, 0xA7, 0x0E, 0x26, 0x59, 0xA8, 0x33, 0xA5, 0xF1, 0x11, 0xDB, 0xCB, 0xD3, 0xA7, 0x1F, 0xB1, 0xC6, 0x10, 0x39, 0xC8, 0x31, 0x1D, 0x60, 0xDB, 0x0D, 0xA4, 0x13, 0x4B, 0x2B, 0x0E, 0xF3, 0x6F, 0x69, 0xCB, 0xA8, 0x62, 0x03, 0x69, 0xE6, 0x95, 0x6B, 0x8D, 0x11, 0xF6, 0xAF, 0xD9, 0xC2, 0x27, 0x3A, 0x32, 0x12, 0x05, 0xC3, 0xB1, 0xE2, 0x81, 0x4B, 0x40, 0xF8, 0x8B, 0x8D, 0xBA, 0x1F, 0x55, 0x60, 0x2C, 0x09, 0xC6, 0xED, 0x73, 0x96, 0x32, 0xAF, 0x5F, 0xEE, 0x8F, 0xEB, 0x5B, 0x93, 0xCF, 0x73, 0x13, 0x15, 0x6B, 0x92, 0x7B, 0x27, 0x0A, 0x13, 0xF0, 0x03, 0x4D, 0x6F, 0x5E, 0x40, 0x7B, 0x9B, 0xD5, 0xCE, 0xFC, 0x04, 0x97, 0x7E, 0xAA, 0xA3, 0x53, 0x2A, 0xCF, 0xD2, 0xD5, 0xCF, 0x52, 0xB2, 0x40, 0x61, 0x28, 0xB1, 0xA6, 0xF6, 0x78, 0xFB, 0x69, 0x9A, 0x85, 0xD6, 0xB9, 0x13, 0x14, 0x6D, 0xC4, 0x25, 0x36, 0x17, 0xDB, 0x54, 0x0C, 0xD8, 0x77, 0x80, 0x9A, 0x00, 0x62, 0x83, 0xDD, 0xB0, 0x06, 0x64, 0xD0, 0x81, 0x5B, 0x0D, 0x23, 0x9E, 0x88, 0xBD],
            DP = null
        };
        private void LoadGameData(string file)
        {
            using var fileStream = File.Open(file, FileMode.Open, FileAccess.Read);

            var a = new Rfc2898DeriveBytes(PresharedValue, GameConfig.Root.StaticData.GetSalt2Bytes(), 10000, HashAlgorithmName.SHA256);
            var key2 = a.GetBytes(32);

            byte[] decryptionKey = key2[0..16];
            byte[] iv = key2[16..32];
            var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Key = decryptionKey;
            aes.IV = iv;
            var transform = aes.CreateDecryptor();
            using CryptoStream stream = new CryptoStream(fileStream, transform, CryptoStreamMode.Read);

            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            var bytes = ms.ToArray();
            var zip = new ZipFile(ms, false);

            var signEntry = zip.GetEntry("sign");
            if (signEntry == null) throw new Exception("error 1");
            var dataEntry = zip.GetEntry("data");
            if (dataEntry == null) throw new Exception("error 2");

            var signStream = zip.GetInputStream(signEntry);
            var dataStream = zip.GetInputStream(dataEntry);

            using MemoryStream signMs = new MemoryStream();
            signStream.CopyTo(signMs);

            using MemoryStream dataMs = new MemoryStream();
            dataStream.CopyTo(dataMs);
            dataMs.Position = 0;

            var rsa = RSA.Create(LoadParameters);
            if (!rsa.VerifyData(dataMs, signMs.ToArray(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                throw new Exception("error 3");

            dataMs.Position = 0;
            var keyDecryptor2 = new Rfc2898DeriveBytes(PresharedValue, GameConfig.Root.StaticData.GetSalt1Bytes(), 10000, HashAlgorithmName.SHA256);
            var key3 = keyDecryptor2.GetBytes(32);

            byte[] val2 = key3[0..16];
            byte[] iv2 = key3[16..32];

            ZipStream = new MemoryStream();
            DoTransformation(val2, iv2, dataMs, ZipStream);

            ZipStream.Position = 0;

            MainZip = new ZipFile(ZipStream, false);
        }

        public static void DoTransformation(byte[] key, byte[] salt, Stream inputStream, Stream outputStream)
        {
            SymmetricAlgorithm aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            int blockSize = aes.BlockSize / 8;

            if (salt.Length != blockSize)
            {
                throw new ArgumentException(
                    "Salt size must be same as block size " +
                    $"(actual: {salt.Length}, expected: {blockSize})");
            }

            var counter = (byte[])salt.Clone();

            var xorMask = new Queue<byte>();

            var zeroIv = new byte[blockSize];
            ICryptoTransform counterEncryptor = aes.CreateEncryptor(key, zeroIv);

            int b;
            while ((b = inputStream.ReadByte()) != -1)
            {
                if (xorMask.Count == 0)
                {
                    var counterModeBlock = new byte[blockSize];

                    counterEncryptor.TransformBlock(
                        counter, 0, counter.Length, counterModeBlock, 0);

                    for (var i2 = counter.Length - 1; i2 >= 0; i2--)
                    {
                        if (++counter[i2] != 0)
                        {
                            break;
                        }
                    }

                    foreach (var b2 in counterModeBlock)
                    {
                        xorMask.Enqueue(b2);
                    }
                }

                var mask = xorMask.Dequeue();
                outputStream.WriteByte((byte)(((byte)b) ^ mask));
            }
        }

        public static async Task Load()
        {
            var targetFile = await AssetDownloadUtil.DownloadOrGetFileAsync(GameConfig.Root.StaticData.Url, CancellationToken.None);
            if (targetFile == null) throw new Exception("static data download fail");

            _instance = new(targetFile);
        }
        #endregion

        private async Task<T> LoadZip<T>(string entry, ProgressBar bar)
        {
            var mainQuestData = MainZip.GetEntry(entry) ?? throw new Exception(entry + " does not exist in static data");
            using StreamReader mainQuestReader = new(MainZip.GetInputStream(mainQuestData));
            var mainQuestDataString = await mainQuestReader.ReadToEndAsync();

            var questdata = JsonConvert.DeserializeObject<T>(mainQuestDataString);
            if (questdata == null) throw new Exception("failed to parse " + entry);

            currentFile++;
            bar.Report((double)currentFile / totalFiles);

            return questdata;
        }

        private async Task<JArray> LoadZip(string entry, ProgressBar bar)
        {
            var mainQuestData = MainZip.GetEntry(entry) ?? throw new Exception(entry + " does not exist in static data");
            using StreamReader mainQuestReader = new(MainZip.GetInputStream(mainQuestData));
            var mainQuestDataString = await mainQuestReader.ReadToEndAsync();

            JObject questdata = JObject.Parse(mainQuestDataString) ?? throw new Exception("failed to parse " + entry);
            JArray? records = (JArray?)questdata["records"] ?? throw new Exception(entry + " is missing records element");
            currentFile++;

            bar.Report((double)currentFile / totalFiles);

            return records;
        }

        int totalFiles = 78;
        int currentFile = 0;

        public async Task Parse()
        {
            using var progress = new ProgressBar();

            var questDataRecords = await LoadZip<MainQuestCompletionTable>("MainQuestTable.json", progress);
            foreach (var obj in questDataRecords.records)
            {
                this.questDataRecords.Add(obj.id, obj);
            }

            var stageDataRecords = await LoadZip<CampaignStageTable>("CampaignStageTable.json", progress);
            foreach (var obj in stageDataRecords.records)
            {
                this.stageDataRecords.Add(obj.id, obj);
            }

            var rewardDataRecords = await LoadZip<RewardTable>("RewardTable.json", progress);
            foreach (var obj in rewardDataRecords.records)
            {
                this.rewardDataRecords.Add(obj.id, obj);
            }

            var chapterCampaignData = await LoadZip<CampaignChapterTable>("CampaignChapterTable.json", progress);
            foreach (var obj in chapterCampaignData.records)
            {
                this.chapterCampaignData.Add(obj.chapter, obj);
            }

            userExpDataRecords = await LoadZip("UserExpTable.json", progress);
            characterCostumeTable = await LoadZip("CharacterCostumeTable.json", progress);

            var characterTable = await LoadZip<CharacterTable>("CharacterTable.json", progress);
            foreach (var obj in characterTable.records)
            {
                this.characterTable.Add(obj.id, obj);
            }

            var tutorialTable = await LoadZip<TutorialTable>("ContentsTutorialTable.json", progress);
            foreach (var obj in tutorialTable.records)
            {
                this.tutorialTable.Add(obj.id, obj);
            }

            var itemEquipTable = await LoadZip<ItemEquipTable>("ItemEquipTable.json", progress);
            foreach (var obj in itemEquipTable.records)
            {
                this.itemEquipTable.Add(obj.id, obj);
            }

            var itemMaterialTable = await LoadZip<ItemMaterialTable>("ItemMaterialTable.json", progress);
            foreach (var obj in itemMaterialTable.records)
            {
                this.itemMaterialTable.Add(obj.id, obj);
            }

            var itemEquipExpTable = await LoadZip<ItemEquipExpTable>("ItemEquipExpTable.json", progress);
            foreach (var obj in itemEquipExpTable.records)
            {
                this.itemEquipExpTable.Add(obj.id, obj);
            }

            var ItemEquipGradeExpTable = await LoadZip<ItemEquipGradeExpTable>("ItemEquipGradeExpTable.json", progress);
            foreach (var obj in ItemEquipGradeExpTable.records)
            {
                this.ItemEquipGradeExpTable.Add(obj.id, obj);
            }

            var characterLevelTable = await LoadZip("CharacterLevelTable.json", progress);

            foreach (JToken item in characterLevelTable)
            {
                var obj = item.ToObject<CharacterLevelData>();
                if (obj != null)
                    LevelData.Add(obj.level, obj);
                else
                    Console.WriteLine("failed to read character level table entry");
            }

            var tacticLessonTable = await LoadZip<TacticAcademyLessonTable>("TacticAcademyFunctionTable.json", progress);
            foreach (var obj in tacticLessonTable.records)
            {
                TacticAcademyLessons.Add(obj.id, obj);
            }

            var sideStoryTable = await LoadZip("SideStoryStageTable.json", progress);

            foreach (JToken item in sideStoryTable)
            {
                var idRaw = item["id"];
                var rewardIdRaw = item["first_clear_reward"];

                if (idRaw == null) throw new InvalidDataException();
                if (rewardIdRaw != null)
                {
                    var id2 = idRaw.ToObject<int>();
                    var reward = rewardIdRaw.ToObject<int>();

                    SidestoryRewardTable.Add(id2, reward);
                }
            }


            foreach (ZipEntry item in MainZip)
            {
                if (item.Name.StartsWith("CampaignMap/") || item.Name.StartsWith("EventMap/"))
                {
                    var x = await LoadZip(item.Name, progress);

                    var items = x[0]["ItemSpawner"];

                    if (items != null)
                        foreach (var item2 in items)
                        {
                            var posId = item2["positionId"] ?? throw new Exception("positionId cannot be null");
                            var rewardObj = item2["itemId"] ?? throw new Exception("itemId cannot be null");

                            var id = posId.ToObject<string>() ?? throw new Exception("positionId cannot be null");
                            var reward = rewardObj.ToObject<int>();

                            PositionReward.TryAdd(id, reward);
                        }
                }
            }
            var fieldItems = await LoadZip<FieldItemTable>("FieldItemTable.json", progress);
            foreach (var obj in fieldItems.records)
            {
                FieldItems.Add(obj.id, obj);
            }
            var battleOutpostTable = await LoadZip<OutpostBattleTable>("OutpostBattleTable.json", progress);
            foreach (var obj in battleOutpostTable.records)
            {
                OutpostBattle.Add(obj.id, obj);
            }
			
			var archiveRecordManagerTableData = await LoadZip<ArchiveRecordManagerTable>("ArchiveRecordManagerTable.json", progress);
			foreach (var obj in archiveRecordManagerTableData.records)
			{
				archiveRecordManagerTable.Add(obj.id, obj);
			}

            var gachaTypeTable = await LoadZip<GachaTypeTable>("GachaTypeTable.json", progress);

            // Add the records to the dictionary
            foreach (var obj in gachaTypeTable.records)
            {
                gachaTypes.Add(obj.id, obj);  // Use obj.id as the key and obj (the GachaType) as the value
            }

            var eventManagerTable = await LoadZip<EventManagerTable>("EventManagerTable.json", progress);

            // Add the records to the dictionary
            foreach (var obj in eventManagerTable.records)
            {
                eventManagers.Add(obj.id, obj);  // Use obj.id as the key and obj (the EventManager) as the value
            }

            var lwptable = await LoadZip<LiveWallpaperTable>("LiveWallpaperTable.json", progress);

            // Add the records to the dictionary
            foreach (var obj in lwptable.records)
            {
                lwptablemgrs.Add(obj.id, obj);  // Use obj.id as the key and obj (the LiveWallpaperRecord) as the value
            }
		
			var userFrameData = await LoadZip<UserFrameTable>("UserFrameTable.json", progress);
			foreach (var record in userFrameData.records)
			{
				userFrameTable[record.id] = record;
			}
			// Load and parse ArchiveEventDungeonStageTable.json
			var archiveEventDungeonStageData = await LoadZip<ArchiveEventDungeonStageTable>("ArchiveEventDungeonStageTable.json", progress);
			foreach (var obj in archiveEventDungeonStageData.records)
			{
				archiveEventDungeonStageRecords.Add(obj.id, obj);
			}
			
			var userTitleTable = await LoadZip<UserTitleTable>("UserTitleTable.json", progress);
			foreach (var obj in userTitleTable.records)
			{
				userTitleRecords.Add(obj.id, obj);
			}

			// Load and parse ArchiveEventStoryTable.json
			var archiveEventStoryTable = await LoadZip<ArchiveEventStoryTable>("ArchiveEventStoryTable.json", progress);
			foreach (var obj in archiveEventStoryTable.records)
			{
				archiveEventStoryRecords.Add(obj.id, obj);
			}

			// Load and parse ArchiveEventQuestTable.json
			var archiveEventQuestTable = await LoadZip<ArchiveEventQuestTable>("ArchiveEventQuestTable.json", progress);
			foreach (var obj in archiveEventQuestTable.records)
			{
				archiveEventQuestRecords.Add(obj.id, obj);
			}
            // LOAD ARCHIVE MESSENGER CONDITION TABLE
            var archiveMessengerConditionTable = await LoadZip<ArchiveMessengerConditionTable>("ArchiveMessengerConditionTable.json", progress);
            foreach (var obj in archiveMessengerConditionTable.records)
            {
                archiveMessengerConditionRecords.Add(obj.id, obj);
            }
            var albumResourceTable = await LoadZip<AlbumResourceTable>("AlbumResourceTable.json", progress);
            foreach (var obj in albumResourceTable.records)
            {
                albumResourceRecords.Add(obj.id, obj);  // Now refers to the class-level field
            }
            // Load Jukebox data
            await LoadJukeboxListData(progress);
            await LoadJukeboxThemeData(progress);

            var characterStatTable = await LoadZip<CharacterStatTable>("CharacterStatTable.json", progress);
            foreach (var obj in characterStatTable.records)
            {
                this.characterStatTable.Add(obj.id, obj);
            }

            var skillinfoTable = await LoadZip<SkillInfoTable>("SkillInfoTable.json", progress);
            foreach (var obj in skillinfoTable.records)
            {
                this.skillInfoTable.Add(obj.id, obj);
            }

            var costTable = await LoadZip<CostTable>("CostTable.json", progress);
            foreach (var obj in costTable.records)
            {
                this.costTable.Add(obj.id, obj);
            }

            var mediasProductTable = await LoadZip<MidasProductTable>("MidasProductTable.json", progress);
            foreach (var obj in mediasProductTable.records)
            {
                this.mediasProductTable.Add(obj.midas_product_id_proximabeta, obj);
            }

            var towerTable = await LoadZip<TowerTable>("TowerTable.json", progress);
            foreach (var obj in towerTable.records)
            {
                this.towerTable.Add(obj.id, obj);
            }
        }

        public async Task LoadJukeboxListData(ProgressBar bar)
        {
            var jukeboxListData = await LoadZip("JukeboxListTable.json", bar);
            foreach (JObject obj in jukeboxListData)
            {
                var record = obj.ToObject<JukeboxListRecord>();
                if (record != null)
                {
                    jukeboxListDataRecords.Add(record.id, record);
                }
            }
        }

        public Dictionary<int, JukeboxListRecord> GetJukeboxListDataRecords()
        {
            return jukeboxListDataRecords;
        }

        public async Task LoadJukeboxThemeData(ProgressBar bar)
        {
            var jukeboxThemeData = await LoadZip("JukeboxThemeTable.json", bar);
            foreach (JObject obj in jukeboxThemeData)
            {
                var record = obj.ToObject<JukeboxThemeRecord>();
                if (record != null)
                {
                    jukeboxThemeDataRecords.Add(record.id, record);
                }
            }
        }

        public MainQuestCompletionRecord? GetMainQuestForStageClearCondition(int stage)
        {
            foreach (var item in questDataRecords)
            {
                if (item.Value.condition_id == stage)
                {
                    return item.Value;
                }
            }

            return null;
        }
        public MainQuestCompletionRecord? GetMainQuestByTableId(int tid)
        {
            return questDataRecords[tid];
        }
        public CampaignStageRecord? GetStageData(int stage)
        {
            return stageDataRecords[stage];
        }
        public RewardTableRecord? GetRewardTableEntry(int rewardId)
        {
            return rewardDataRecords[rewardId];
        }
        /// <summary>
        /// Returns the level and its minimum value for XP value
        /// </summary>
        /// <param name="targetExp"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (int, int) GetUserLevelFromUserExp(int targetExp)
        {
            int prevLevel = 0;
            int prevValue = 0;
            for (int i = 0; i < userExpDataRecords.Count; i++)
            {
                var item = userExpDataRecords[i];

                var level = item["level"];
                if (level == null) throw new Exception("expected level field in user exp table data");

                int levelValue = level.ToObject<int>();

                var exp = item["exp"];
                if (exp == null) throw new Exception("expected exp field in user exp table data");

                int expValue = exp.ToObject<int>();

                if (prevValue < targetExp)
                {
                    prevLevel = levelValue;
                    prevValue = expValue;
                }
                else
                {
                    return (prevLevel, prevValue);
                }
            }
            return (-1, -1);
        }
        public int GetUserMinXpForLevel(int targetLevel)
        {
            for (int i = 0; i < userExpDataRecords.Count; i++)
            {
                var item = userExpDataRecords[i];

                var level = item["level"];
                if (level == null) throw new Exception("expected level field in user exp table data");

                int levelValue = level.ToObject<int>();
                if (targetLevel == levelValue)
                {
                    var exp = item["exp"];
                    if (exp == null) throw new Exception("expected exp field in user exp table data");

                    int expValue = exp.ToObject<int>();
                    return expValue;
                }
            }
            return -1;
        }
        public int GetNormalChapterNumberFromFieldName(string field)
        {
            foreach (var item in chapterCampaignData)
            {
                if (item.Value.field_id == field)
                {
                    return item.Value.chapter;
                }
            }

            return -1;
        }

        public IEnumerable<int> GetAllCharacterTids()
        {
            return characterTable.Keys;
        }
        public IEnumerable<int> GetAllCostumes()
        {
            foreach (JObject item in characterCostumeTable)
            {
                var id = item["id"];
                if (id == null) throw new Exception("expected id field in reward data");

                int value = id.ToObject<int>();
                yield return value;
            }
        }

        internal ClearedTutorialData GetTutorialDataById(int TableId)
        {
            return tutorialTable[TableId];
        }

        public string? GetItemSubType(int itemType)
        {
            return itemEquipTable[itemType].item_sub_type;
        }

        internal IEnumerable<int> GetStageIdsForChapter(int chapterNumber, bool normal)
        {
            string mod = normal ? "Normal" : "Hard";
            foreach (var item in stageDataRecords)
            {
                var data = item.Value;

                int chVal = data.chapter_id - 1;

                if (chapterNumber == chVal && data.chapter_mod == mod && data.stage_type == "Main")
                {
                    yield return data.id;
                }
            }
        }

        public Dictionary<int, CharacterLevelData> GetCharacterLevelUpData()
        {
            return LevelData;
        }

        public TacticAcademyLessonRecord GetTacticAcademyLesson(int lessonId)
        {
            return TacticAcademyLessons[lessonId];
        }

        // Methods to access Jukebox data
        public JukeboxListRecord? GetJukeboxListRecordById(int id)
        {
            jukeboxListDataRecords.TryGetValue(id, out var record);
            return record;
        }

        public JukeboxThemeRecord? GetJukeboxThemeRecordById(int id)
        {
            jukeboxThemeDataRecords.TryGetValue(id, out var record);
            return record;
        }

        public IEnumerable<string> GetScenarioStageIdsForChapter(int chapterNumber)
        {

            return albumResourceRecords.Values.Where(record => record.target_chapter == chapterNumber && !string.IsNullOrEmpty(record.scenario_group_id)).Select(record => record.scenario_group_id);
        }
        public bool IsValidScenarioStage(string scenarioGroupId, int targetChapter, int targetStage)
        {
            // Only process stages that belong to the main quest
            if (!scenarioGroupId.StartsWith("d_main_"))
            {
                return false; // Exclude stages that don't belong to the main quest
            }

            // Example regular stage format: "d_main_26_08"
            // Example bonus stage format: "d_main_18af_06"
            // Example stage with suffix format: "d_main_01_01_s" or "d_main_01_01_e"

            var parts = scenarioGroupId.Split('_');

            if (parts.Length < 4)
            {
                return false; // If it doesn't have at least 4 parts, it's not a valid stage
            }

            string chapterPart = parts[2]; // This could be "26", "18af", "01"
            string stagePart = parts[3];   // This is the stage part, e.g., "08", "01_s", or "01_e"

            // Remove any suffixes like "_s", "_e" from the stage part for comparison
            string cleanedStagePart = stagePart.Split('_')[0];  // Removes "_s", "_e", etc.

            // Handle bonus stages (ending in "af" or having "_s", "_e" suffix)
            bool isBonusStage = chapterPart.EndsWith("af") || stagePart.Contains("_s") || stagePart.Contains("_e");

            // Extract chapter number (remove "af" if present)
            string chapterNumberStr = isBonusStage && chapterPart.EndsWith("af")
                ? chapterPart.Substring(0, chapterPart.Length - 2)  // Remove "af"
                : chapterPart;

            // Parse chapter and stage numbers
            if (int.TryParse(chapterNumberStr, out int chapter) && int.TryParse(cleanedStagePart, out int stage))
            {
                // Check if it's a bonus stage with a suffix
                bool isSpecialStage = stagePart.Contains("_s") || stagePart.Contains("_e");

                // Only accept stages if they are:
                // 1. In a chapter less than the target chapter
                // 2. OR in the target chapter but with a stage number less than or equal to the target stage
                // 3. OR it's a special stage (with "_s" or "_e") in the target chapter and target stage
                if (chapter < targetChapter ||
                    (chapter == targetChapter && (stage < targetStage || (stage == targetStage && isSpecialStage))))
                {
                    return true;
                }
            }

            return false;
        }


    }
}
