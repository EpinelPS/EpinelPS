using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using EpinelPS.Database;
using EpinelPS.Utils;
using ICSharpCode.SharpZipLib.Zip;
using MemoryPack;
using Newtonsoft.Json;

namespace EpinelPS.Data
{
    public class GameData
    {
        private static GameData? _instance;
        public static GameData Instance
        {
            get
            {
                _instance ??= BuildAsync().Result;

                return _instance;
            }
        }

        public byte[] Sha256Hash;
        public byte[] MpkHash = [];
        public int Size;
        public int MpkSize;

        private ZipFile MainZip;
        private MemoryStream ZipStream;
        private int totalFiles = 1;
        private int currentFile;

        // TODO: all of the data types need to be changed to match the game
        private bool UseMemoryPack = true;

        public readonly Dictionary<string, MapInfo> MapData = [];

        [LoadRecord("MainQuestTable.json", "id")]
        public readonly Dictionary<int, MainQuestCompletionRecord> QuestDataRecords = [];

        [LoadRecord("CampaignStageTable.json", "id")]
        public readonly Dictionary<int, CampaignStageRecord> StageDataRecords = [];

        [LoadRecord("RewardTable.json", "id")]
        public readonly Dictionary<int, RewardTableRecord> RewardDataRecords = [];

        [LoadRecord("UserExpTable.json", "level")]
        public readonly Dictionary<int, UserExpRecord> UserExpDataRecords = [];

        [LoadRecord("CampaignChapterTable.json", "chapter")]
        public readonly Dictionary<int, CampaignChapterRecord> ChapterCampaignData = [];

        [LoadRecord("CharacterCostumeTable.json", "id")]
        public readonly Dictionary<int, CharacterCostumeRecord> CharacterCostumeTable = [];

        [LoadRecord("CharacterTable.json", "id")]
        public readonly Dictionary<int, CharacterRecord> CharacterTable = [];

        [LoadRecord("ContentsTutorialTable.json", "id")]
        public readonly Dictionary<int, ClearedTutorialData> TutorialTable = [];
        [LoadRecord("ItemEquipTable.json", "id")]

        public readonly Dictionary<int, ItemEquipRecord> ItemEquipTable = [];

        [LoadRecord("ItemMaterialTable.json", "id")]
        public readonly Dictionary<int, ItemMaterialRecord> itemMaterialTable = [];

        [LoadRecord("ItemEquipExpTable.json", "id")]
        public readonly Dictionary<int, ItemEquipExpRecord> itemEquipExpTable = [];

        [LoadRecord("ItemEquipGradeExpTable.json", "id")]
        public readonly Dictionary<int, ItemEquipGradeExpRecord> ItemEquipGradeExpTable = [];

        [LoadRecord("CharacterLevelTable.json", "level")]
        public readonly Dictionary<int, CharacterLevelData> LevelData = [];

        [LoadRecord("TacticAcademyFunctionTable.json", "id")]
        public readonly Dictionary<int, TacticAcademyLessonRecord> TacticAcademyLessons = [];

        [LoadRecord("SideStoryStageTable.json", "id")]
        public readonly Dictionary<int, SideStoryStageRecord> SidestoryRewardTable = [];

        [LoadRecord("FieldItemTable.json", "id")]
        public readonly Dictionary<int, FieldItemRecord> FieldItems = [];

        [LoadRecord("OutpostBattleTable.json", "id")]
        public readonly Dictionary<int, OutpostBattleTableRecord> OutpostBattle = [];

        [LoadRecord("JukeboxListTable.json", "id")]
        public readonly Dictionary<int, JukeboxListRecord> jukeboxListDataRecords = [];

        [LoadRecord("JukeboxThemeTable.json", "id")]
        public readonly Dictionary<int, JukeboxThemeRecord> jukeboxThemeDataRecords = [];

        [LoadRecord("GachaTypeTable.json", "id")]
        public readonly Dictionary<int, GachaType> gachaTypes = [];

        [LoadRecord("EventManagerTable.json", "id")]
        public readonly Dictionary<int, EventManager> eventManagers = [];

        [LoadRecord("LiveWallpaperTable.json", "id")]
        public readonly Dictionary<int, LiveWallpaperRecord> lwptablemgrs = [];

        [LoadRecord("AlbumResourceTable.json", "id")]
        public readonly Dictionary<int, AlbumResourceRecord> albumResourceRecords = [];

        [LoadRecord("UserFrameTable.json", "id")]
        public readonly Dictionary<int, UserFrameTableRecord> userFrameTable = [];

        [LoadRecord("ArchiveRecordManagerTable.json", "id")]
        public readonly Dictionary<int, ArchiveRecordManagerRecord> archiveRecordManagerTable = [];

        [LoadRecord("ArchiveEventStoryTable.json", "id")]
        public readonly Dictionary<int, ArchiveEventStoryRecord> archiveEventStoryRecords = [];

        [LoadRecord("ArchiveEventQuestTable.json", "id")]
        public readonly Dictionary<int, ArchiveEventQuestRecord> archiveEventQuestRecords = [];

        [LoadRecord("ArchiveEventDungeonStageTable.json", "id")]
        public readonly Dictionary<int, ArchiveEventDungeonStageRecord> archiveEventDungeonStageRecords = [];

        [LoadRecord("UserTitleTable.json", "id")]
        public readonly Dictionary<int, UserTitleRecord> userTitleRecords = [];

        [LoadRecord("ArchiveMessengerConditionTable.json", "id")]
        public readonly Dictionary<int, ArchiveMessengerConditionRecord> archiveMessengerConditionRecords = [];

        [LoadRecord("CharacterStatTable.json", "id")]
        public readonly Dictionary<int, CharacterStatRecord> characterStatTable = [];

        [LoadRecord("SkillInfoTable.json", "id")]
        public readonly Dictionary<int, SkillInfoRecord> skillInfoTable = [];

        [LoadRecord("CostTable.json", "id")]
        public readonly Dictionary<int, CostRecord> costTable = [];

        [LoadRecord("MidasProductTable.json", "midas_product_id_proximabeta")]
        public readonly Dictionary<string, MidasProductRecord> mediasProductTable = [];

        [LoadRecord("TowerTable.json", "id")]
        public readonly Dictionary<int, TowerRecord> towerTable = [];

        [LoadRecord("TriggerTable.json", "id")]
        public readonly Dictionary<int, TriggerRecord> TriggerTable = [];

        [LoadRecord("InfraCoreGradeTable.json", "id")]
        public readonly Dictionary<int, InfracoreRecord> InfracoreTable = [];

        [LoadRecord("AttractiveCounselCharacterTable.json", "name_code")]
        public readonly Dictionary<int, AttractiveCounselCharacterRecord> AttractiveCounselCharacterTable = [];

        [LoadRecord("AttractiveLevelRewardTable.json", "id")]
        public readonly Dictionary<int, AttractiveLevelRewardRecord> AttractiveLevelReward = [];

        [LoadRecord("AttractiveLevelTable.json", "id")]
        public readonly Dictionary<int, AttractiveLevelRecord> AttractiveLevelTable = [];

        [LoadRecord("SubQuestTable.json", "id")]
        public readonly Dictionary<int, SubquestRecord> Subquests = [];

        [LoadRecord("MessengerDialogTable.json", "id")]
        public readonly Dictionary<string, MessengerDialogRecord> Messages = [];

        [LoadRecord("MessengerConditionTriggerTable.json", "id")]
        public readonly Dictionary<int, MessengerMsgConditionRecord> MessageConditions = [];

        [LoadRecord("ScenarioRewardsTable.json", "condition_id")]
        public readonly Dictionary<string, ScenarioRewardRecord> ScenarioRewards = [];

        // Note: same data types are intentional
        [LoadRecord("ProductOfferTable.json", "id")]
        public readonly Dictionary<int, ProductOfferRecord> ProductOffers = [];

        [LoadRecord("PopupPackageListTable.json", "id")]
        public readonly Dictionary<int, ProductOfferRecord> PopupPackages = [];

        [LoadRecord("InterceptNormalTable.json", "id")]
        public readonly Dictionary<int, InterceptionRecord> InterceptNormal = [];

        [LoadRecord("InterceptSpecialTable.json", "id")]
        public readonly Dictionary<int, InterceptionRecord> InterceptSpecial = [];

        [LoadRecord("ConditionRewardTable.json", "id")]
        public readonly Dictionary<int, ConditionRewardRecord> ConditionRewards = [];
        [LoadRecord("ItemConsumeTable.json", "id")]
        public readonly Dictionary<int, ItemConsumeRecord> ConsumableItems = [];
        [LoadRecord("ItemRandomTable.json", "id")]
        public readonly Dictionary<int, RandomItemRecord> RandomItem = [];
        [LoadRecord("LostSectorTable.json", "id")]
        public readonly Dictionary<int, LostSectorRecord> LostSector = [];
        [LoadRecord("LostSectorStageTable.json", "id")]
        public readonly Dictionary<int, LostSectorStageRecord> LostSectorStages = [];
        [LoadRecord("ItemPieceTable.json", "id")]
        public readonly Dictionary<int, ItemPieceRecord> PieceItems = [];
        [LoadRecord("GachaGradeProbTable.json", "id")]
        public readonly Dictionary<int, GachaGradeProbRecord> GachaGradeProb = [];
        [LoadRecord("GachaListProbTable.json", "id")]
        public readonly Dictionary<int, GachaListProbRecord> GachaListProb = [];
        [LoadRecord("RecycleResearchStatTable.json", "id")]
        public readonly Dictionary<int, RecycleResearchStatRecord> RecycleResearchStats = [];
        [LoadRecord("RecycleResearchLevelTable.json", "id")]
        public readonly Dictionary<int, RecycleResearchLevelRecord> RecycleResearchLevels = [];

        // Harmony Cube  Data Tables
        [LoadRecord("ItemHarmonyCubeTable.json", "id")]
        public readonly Dictionary<int, ItemHarmonyCubeRecord> ItemHarmonyCubeTable = [];

        [LoadRecord("ItemHarmonyCubeLevelTable.json", "id")]
        public readonly Dictionary<int, ItemHarmonyCubeLevelRecord> ItemHarmonyCubeLevelTable = [];

        // Favorite Item  Data Tables
        [LoadRecord("FavoriteItemTable.json", "id")]
        public readonly Dictionary<int, FavoriteItemRecord> FavoriteItemTable = [];

        [LoadRecord("FavoriteItemExpTable.json", "id")]
        public readonly Dictionary<int, FavoriteItemExpRecord> FavoriteItemExpTable = [];

        [LoadRecord("FavoriteItemLevelTable.json", "id")]
        public readonly Dictionary<int, FavoriteItemLevelRecord> FavoriteItemLevelTable = [];

        [LoadRecord("FavoriteItemProbabilityTable.json", "id")]
        public readonly Dictionary<int, FavoriteItemProbabilityRecord> FavoriteItemProbabilityTable = [];

        [LoadRecord("FavoriteItemQuestTable.json", "id")]
        public readonly Dictionary<int, FavoriteItemQuestRecord> FavoriteItemQuestTable = [];

        [LoadRecord("FavoriteItemQuestStageTable.json", "id")]
        public readonly Dictionary<int, FavoriteItemQuestStageRecord> FavoriteItemQuestStageTable = [];

        // Tables related to PlaySoda Arcade's event.

        [LoadRecord("EventPlaySodaManagerTable.json", "id")]
        public readonly Dictionary<int, EventPlaySodaManagerRecord> EventPlaySodaManagerTable = [];

        [LoadRecord("EventPlaySodaStoryModeTable.json", "id")]
        public readonly Dictionary<int, EventPlaySodaStoryModeRecord> EventPlaySodaStoryModeTable = [];

        [LoadRecord("EventPlaySodaChallengeModeTable.json", "id")]
        public readonly Dictionary<int, EventPlaySodaChallengeModeRecord> EventPlaySodaChallengeModeTable = [];

        [LoadRecord("EventPlaySodaPointRewardTable.json", "id")]
        public readonly Dictionary<int, EventPlaySodaPointRewardRecord> EventPlaySodaPointRewardTable = [];

        // Tables related to InTheMirror Arcade's event.

        [LoadRecord("EventMvgQuestTable.json", "id")]
        public readonly Dictionary<int, EventMVGQuestRecord_Raw> EventMvgQuestTable = [];

        [LoadRecord("EventMvgQuestTable.json", "id")]
        public readonly Dictionary<int, EventMVGShopRecord_Raw> EventMvgShopTable = [];

        [LoadRecord("EventMVGMissionTable.json", "id")]
        public readonly Dictionary<int, EventMVGMissionRecord_Raw> EventMvgMissionTable = [];

        static async Task<GameData> BuildAsync()
        {
            await Load();

            Logging.WriteLine("Preparing");
            Stopwatch stopWatch = new();
            stopWatch.Start();
            await Instance.Parse();

            stopWatch.Stop();
            Logging.WriteLine("Preparing took " + stopWatch.Elapsed);
            return Instance;
        }

        public GameData(string filePath, string mpkFilePath)
        {
            if (!File.Exists(filePath)) throw new ArgumentException("Static data file must exist", nameof(filePath));

            // disable warnings
            ZipStream = new();

            // process json data
            byte[] rawBytes = File.ReadAllBytes(filePath);
            Sha256Hash = SHA256.HashData(rawBytes);
            Size = rawBytes.Length;

            // process mpk data
            if (!string.IsNullOrEmpty(mpkFilePath))
            {
                byte[] rawBytes2 = File.ReadAllBytes(mpkFilePath);
                MpkHash = SHA256.HashData(rawBytes2);
                MpkSize = rawBytes2.Length;
            }

            if (UseMemoryPack)
                LoadGameData(mpkFilePath, GameConfig.Root.StaticDataMpk);
            else
                LoadGameData(filePath, GameConfig.Root.StaticData);
            if (MainZip == null) throw new Exception("failed to read zip file");
        }

        #region Data loading
        private static byte[] PresharedValue = [0xCB, 0xC2, 0x1C, 0x6F, 0xF3, 0xF5, 0x07, 0xF5, 0x05, 0xBA, 0xCA, 0xD4, 0x98, 0x28, 0x84, 0x1F, 0xF0, 0xD1, 0x38, 0xC7, 0x61, 0xDF, 0xD6, 0xE6, 0x64, 0x9A, 0x85, 0x13, 0x3E, 0x1A, 0x6A, 0x0C, 0x68, 0x0E, 0x2B, 0xC4, 0xDF, 0x72, 0xF8, 0xC6, 0x55, 0xE4, 0x7B, 0x14, 0x36, 0x18, 0x3B, 0xA7, 0xD1, 0x20, 0x81, 0x22, 0xD1, 0xA9, 0x18, 0x84, 0x65, 0x13, 0x0B, 0xED, 0xA3, 0x00, 0xE5, 0xD9];
        private static RSAParameters LoadParameters = new() 
        {
            Exponent = [0x01, 0x00, 0x01],
            Modulus = [0x89, 0xD6, 0x66, 0x00, 0x7D, 0xFC, 0x7D, 0xCE, 0x83, 0xA6, 0x62, 0xE3, 0x1A, 0x5E, 0x9A, 0x53, 0xC7, 0x8A, 0x27, 0xF3, 0x67, 0xC1, 0xF3, 0xD4, 0x37, 0xFE, 0x50, 0x6D, 0x38, 0x45, 0xDF, 0x7E, 0x73, 0x5C, 0xF4, 0x9D, 0x40, 0x4C, 0x8C, 0x63, 0x21, 0x97, 0xDF, 0x46, 0xFF, 0xB2, 0x0D, 0x0E, 0xDB, 0xB2, 0x72, 0xB4, 0xA8, 0x42, 0xCD, 0xEE, 0x48, 0x06, 0x74, 0x4F, 0xE9, 0x56, 0x6E, 0x9A, 0xB1, 0x60, 0x18, 0xBC, 0x86, 0x0B, 0xB6, 0x32, 0xA7, 0x51, 0x00, 0x85, 0x7B, 0xC8, 0x72, 0xCE, 0x53, 0x71, 0x3F, 0x64, 0xC2, 0x25, 0x58, 0xEF, 0xB0, 0xC9, 0x1D, 0xE3, 0xB3, 0x8E, 0xFC, 0x55, 0xCF, 0x8B, 0x02, 0xA5, 0xC8, 0x1E, 0xA7, 0x0E, 0x26, 0x59, 0xA8, 0x33, 0xA5, 0xF1, 0x11, 0xDB, 0xCB, 0xD3, 0xA7, 0x1F, 0xB1, 0xC6, 0x10, 0x39, 0xC8, 0x31, 0x1D, 0x60, 0xDB, 0x0D, 0xA4, 0x13, 0x4B, 0x2B, 0x0E, 0xF3, 0x6F, 0x69, 0xCB, 0xA8, 0x62, 0x03, 0x69, 0xE6, 0x95, 0x6B, 0x8D, 0x11, 0xF6, 0xAF, 0xD9, 0xC2, 0x27, 0x3A, 0x32, 0x12, 0x05, 0xC3, 0xB1, 0xE2, 0x81, 0x4B, 0x40, 0xF8, 0x8B, 0x8D, 0xBA, 0x1F, 0x55, 0x60, 0x2C, 0x09, 0xC6, 0xED, 0x73, 0x96, 0x32, 0xAF, 0x5F, 0xEE, 0x8F, 0xEB, 0x5B, 0x93, 0xCF, 0x73, 0x13, 0x15, 0x6B, 0x92, 0x7B, 0x27, 0x0A, 0x13, 0xF0, 0x03, 0x4D, 0x6F, 0x5E, 0x40, 0x7B, 0x9B, 0xD5, 0xCE, 0xFC, 0x04, 0x97, 0x7E, 0xAA, 0xA3, 0x53, 0x2A, 0xCF, 0xD2, 0xD5, 0xCF, 0x52, 0xB2, 0x40, 0x61, 0x28, 0xB1, 0xA6, 0xF6, 0x78, 0xFB, 0x69, 0x9A, 0x85, 0xD6, 0xB9, 0x13, 0x14, 0x6D, 0xC4, 0x25, 0x36, 0x17, 0xDB, 0x54, 0x0C, 0xD8, 0x77, 0x80, 0x9A, 0x00, 0x62, 0x83, 0xDD, 0xB0, 0x06, 0x64, 0xD0, 0x81, 0x5B, 0x0D, 0x23, 0x9E, 0x88, 0xBD],
            DP = null
        };
        private void LoadGameData(string file, StaticData data)
        {
            using FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);

            Rfc2898DeriveBytes a = new(PresharedValue, data.GetSalt2Bytes(), 10000, HashAlgorithmName.SHA256);
            byte[] key2 = a.GetBytes(32);

            byte[] decryptionKey = key2[0..16];
            byte[] iv = key2[16..32];
            Aes aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Key = decryptionKey;
            aes.IV = iv;
            ICryptoTransform transform = aes.CreateDecryptor();
            using CryptoStream stream = new(fileStream, transform, CryptoStreamMode.Read);

            using MemoryStream ms = new();
            stream.CopyTo(ms);

            byte[] bytes = ms.ToArray();
            ZipFile zip = new(ms, false);

            ZipEntry signEntry = zip.GetEntry("sign") ?? throw new Exception("error 1");
            ZipEntry dataEntry = zip.GetEntry("data") ?? throw new Exception("error 2");
            Stream signStream = zip.GetInputStream(signEntry);
            Stream dataStream = zip.GetInputStream(dataEntry);

            using MemoryStream signMs = new();
            signStream.CopyTo(signMs);

            using MemoryStream dataMs = new();
            dataStream.CopyTo(dataMs);
            dataMs.Position = 0;

            RSA rsa = RSA.Create(LoadParameters);
            if (!rsa.VerifyData(dataMs, signMs.ToArray(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                throw new Exception("error 3");

            dataMs.Position = 0;
            Rfc2898DeriveBytes keyDec2 = new(PresharedValue, data.GetSalt1Bytes(), 10000, HashAlgorithmName.SHA256);
            byte[] key3 = keyDec2.GetBytes(32);

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

            byte[] counter = (byte[])salt.Clone();

            Queue<byte> xorMask = new();

            byte[] zeroIv = new byte[blockSize];
            ICryptoTransform counterEncryptor = aes.CreateEncryptor(key, zeroIv);

            int b;
            while ((b = inputStream.ReadByte()) != -1)
            {
                if (xorMask.Count == 0)
                {
                    byte[] counterModeBlock = new byte[blockSize];

                    counterEncryptor.TransformBlock(
                        counter, 0, counter.Length, counterModeBlock, 0);

                    for (int i2 = counter.Length - 1; i2 >= 0; i2--)
                    {
                        if (++counter[i2] != 0)
                        {
                            break;
                        }
                    }

                    foreach (byte b2 in counterModeBlock)
                    {
                        xorMask.Enqueue(b2);
                    }
                }

                byte mask = xorMask.Dequeue();
                outputStream.WriteByte((byte)(((byte)b) ^ mask));
            }
        }

        public static async Task Load()
        {
            string? targetFile = await AssetDownloadUtil.DownloadOrGetFileAsync(GameConfig.Root.StaticData.Url, CancellationToken.None) ?? throw new Exception("static data download fail");
            if (string.IsNullOrEmpty(GameConfig.Root.StaticDataMpk.Url))
            {
                _instance = new(targetFile, "");
                return;
            }

            string? targetFile2 = await AssetDownloadUtil.DownloadOrGetFileAsync(GameConfig.Root.StaticDataMpk.Url, CancellationToken.None) ?? throw new Exception("static data download fail");
            _instance = new(targetFile, targetFile2);
        }
        #endregion

        public async Task<X[]> LoadZip<X>(string entry, IProgress<double> bar) where X : new()
        {
            try
            {
                if (UseMemoryPack) entry = entry.Replace(".json", ".mpk");

                ZipEntry fileEntry = MainZip.GetEntry(entry);
                if (fileEntry == null)
                {
                    Logging.WriteLine(entry + " does not exist in static data", LogType.Error);
                    return [];
                }

                X[]? deserializedObject;

                if (UseMemoryPack)
                {
                    Stream stream = MainZip.GetInputStream(fileEntry);
                    deserializedObject = await MemoryPackSerializer.DeserializeAsync<X[]>(stream);
                }
                else
                {
                    using var streamReader = new System.IO.StreamReader(MainZip.GetInputStream(fileEntry));
                    var json = await streamReader.ReadToEndAsync();
                    DataTable<X> obj = JsonConvert.DeserializeObject<DataTable<X>>(json) ?? throw new Exception("deserializeobject failed");
                    deserializedObject = [.. obj.records];
                }

                if (deserializedObject == null) throw new Exception("failed to parse " + entry);

                currentFile++;
                bar.Report((double)currentFile / totalFiles);

                return deserializedObject;
            }
            catch(Exception ex)
            {
                Logging.WriteLine($"Failed to parse {entry}:\n{ex}\n", LogType.Error);
                return [];
            }
        }

        public async Task Parse()
        {
            using ProgressBar progress = new();

            totalFiles = GameDataInitializer.TotalFiles;
            if (totalFiles == 0) throw new Exception("Source generator failed.");

            await GameDataInitializer.InitializeGameData(progress);

            foreach (ZipEntry item in MainZip)
            {
                if (item.Name.StartsWith("CampaignMap/") ||
                    item.Name.StartsWith("EventMap/") ||
                    item.Name.StartsWith("LostSectorMap/")
                   )
                {
                    MapInfo[] x = await LoadZip<MapInfo>(item.Name, progress);

                    foreach (MapInfo map in x)
                    {
                        MapData.Add(map.id, map);
                    }
                }
            }




            // sanity checks
            if (QuestDataRecords.Count == 0) throw new Exception("QuestDataRecords should not be empty");
        }

        public MainQuestCompletionRecord? GetMainQuestForStageClearCondition(int stage)
        {
            if (QuestDataRecords.Count == 0) throw new Exception("QuestDataRecords should not be empty");
            foreach (KeyValuePair<int, MainQuestCompletionRecord> item in QuestDataRecords)
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
            return QuestDataRecords[tid];
        }
        public CampaignStageRecord? GetStageData(int stage)
        {
            return StageDataRecords[stage];
        }
        public RewardTableRecord? GetRewardTableEntry(int rewardId)
        {
            return RewardDataRecords[rewardId];
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
            for (int i = 1; i < UserExpDataRecords.Count + 1; i++)
            {
                UserExpRecord item = UserExpDataRecords[i];

                if (prevValue < targetExp)
                {
                    prevLevel = item.level;
                    prevValue = item.exp;
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
            for (int i = 1; i < UserExpDataRecords.Count + 1; i++)
            {
                UserExpRecord item = UserExpDataRecords[i];

                if (targetLevel == item.level)
                {
                    return item.exp;
                }
            }
            return -1;
        }
        public string? GetMapIdFromDBFieldName(string field)
        {
            // Get game map ID from DB Field Name (ex: 1_Normal for chapter 1 normal)
            string[] keys = field.Split("_");
            if (int.TryParse(keys[0], out int chapterNum))
            {
                string difficulty = keys[1];

                foreach (KeyValuePair<int, CampaignChapterRecord> item in ChapterCampaignData)
                {
                    if (difficulty == "Normal" && item.Value.chapter == chapterNum)
                    {
                        return item.Value.field_id;
                    }
                    else if (difficulty == "Hard" && item.Value.chapter == chapterNum)
                    {
                        return item.Value.hard_field_id;
                    }
                }

                return null;
            }
            else
            {
                return keys[0]; // Already a Map ID
            }
        }
        public int GetNormalChapterNumberFromFieldName(string field)
        {
            foreach (KeyValuePair<int, CampaignChapterRecord> item in ChapterCampaignData)
            {
                if (item.Value.field_id == field)
                {
                    return item.Value.chapter;
                }
            }

            return -1;
        }
        public IEnumerable<int> GetAllCostumes()
        {
            foreach (KeyValuePair<int, CharacterCostumeRecord> item in CharacterCostumeTable)
            {
                yield return item.Value.id;
            }
        }

        internal ClearedTutorialData GetTutorialDataById(int TableId)
        {
            return TutorialTable[TableId];
        }

        public string? GetItemSubType(int itemType)
        {
            // Check if it's an equipment item
            if (ItemEquipTable.TryGetValue(itemType, out ItemEquipRecord? equipRecord))
            {
                return equipRecord.item_sub_type;
            }

            // Check if it's a harmony cube item
            if (ItemHarmonyCubeTable.TryGetValue(itemType, out ItemHarmonyCubeRecord? harmonyCubeRecord))
            {
                return harmonyCubeRecord.item_sub_type;
            }

            // Return null if item type not found
            return null;
        }

        internal IEnumerable<int> GetStageIdsForChapter(int chapterNumber, bool normal)
        {
            ChapterMod mod = normal ? ChapterMod.Normal : ChapterMod.Hard;
            foreach (KeyValuePair<int, CampaignStageRecord> item in StageDataRecords)
            {
                CampaignStageRecord data = item.Value;

                int chVal = data.chapter_id - 1;

                if (chapterNumber == chVal && data.chapter_mod == mod && data.stage_type == StageType.Main)
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

            string[] parts = scenarioGroupId.Split('_');

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
                ? chapterPart[..^2]  // Remove "af"
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

        internal string GetMapIdFromChapter(int chapter, ChapterMod mod)
        {
            CampaignChapterRecord data = ChapterCampaignData[chapter - 1];
            if (mod == ChapterMod.Hard)
                return data.hard_field_id;
            else return data.field_id;
        }
        internal string GetMapIdFromChapter(int chapter, string mod)
        {
            CampaignChapterRecord data = ChapterCampaignData[chapter - 1];
            if (mod == "Hard")
                return data.hard_field_id;
            else return data.field_id;
        }

        internal int GetConditionReward(int groupId, long damage)
        {
            IEnumerable<KeyValuePair<int, ConditionRewardRecord>> results = ConditionRewards.Where(x => x.Value.group == groupId && x.Value.value_min <= damage && x.Value.value_max >= damage);
            if (results.Any())
                return results.FirstOrDefault().Value.reward_id;
            else return 0;
        }

        public FavoriteItemQuestRecord? GetFavoriteItemQuestTableData(int questId)
        {
            FavoriteItemQuestTable.TryGetValue(questId, out FavoriteItemQuestRecord?data);
            return data;
        }

        public FavoriteItemQuestStageRecord? GetFavoriteItemQuestStageData(int stageId)
        {
            FavoriteItemQuestStageTable.TryGetValue(stageId, out FavoriteItemQuestStageRecord? data);
            return data;
        }
    }
}