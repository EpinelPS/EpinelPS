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

        public byte[] MpkHash = [];
        public int MpkSize;

        private ZipFile MainZip;
        private MemoryStream ZipStream;
        private int totalFiles = 1;
        private int currentFile;

        public readonly Dictionary<string, FieldMapRecord> MapData = [];

        [LoadRecord("MainQuestTable.json", "Id")]
        public readonly Dictionary<int, MainQuestRecord> QuestDataRecords = [];

        [LoadRecord("CampaignStageTable.json", "Id")]
        public readonly Dictionary<int, CampaignStageRecord> StageDataRecords = [];

        [LoadRecord("RewardTable.json", "Id")]
        public readonly Dictionary<int, RewardRecord> RewardDataRecords = [];

        [LoadRecord("UserExpTable.json", "Level")]
        public readonly Dictionary<int, UserExpRecord> UserExpDataRecords = [];

        [LoadRecord("CampaignChapterTable.json", "Chapter")]
        public readonly Dictionary<int, CampaignChapterRecord> ChapterCampaignData = [];

        [LoadRecord("CharacterCostumeTable.json", "Id")]
        public readonly Dictionary<int, CharacterCostumeRecord> CharacterCostumeTable = [];

        [LoadRecord("CharacterTable.json", "Id")]
        public readonly Dictionary<int, CharacterRecord> CharacterTable = [];

        [LoadRecord("ContentsTutorialTable.json", "Id")]
        public readonly Dictionary<int, ContentsTutorialRecord> TutorialTable = [];
        [LoadRecord("ItemEquipTable.json", "Id")]

        public readonly Dictionary<int, ItemEquipRecord> ItemEquipTable = [];

        [LoadRecord("ItemMaterialTable.json", "Id")]
        public readonly Dictionary<int, ItemMaterialRecord> itemMaterialTable = [];

        [LoadRecord("ItemEquipExpTable.json", "Id")]
        public readonly Dictionary<int, ItemEquipExpRecord> itemEquipExpTable = [];

        [LoadRecord("ItemEquipGradeExpTable.json", "Id")]
        public readonly Dictionary<int, ItemEquipGradeExpRecord> ItemEquipGradeExpTable = [];

        [LoadRecord("CharacterLevelTable.json", "Level")]
        public readonly Dictionary<int, CharacterLevelRecord> LevelData = [];

        [LoadRecord("TacticAcademyFunctionTable.json", "Id")]
        public readonly Dictionary<int, TacticAcademyFunctionRecord> TacticAcademyLessons = [];

        [LoadRecord("SIdeStoryStageTable.json", "Id")]
        public readonly Dictionary<int, SideStoryStageRecord> SidestoryRewardTable = [];

        [LoadRecord("FieldItemTable.json", "Id")]
        public readonly Dictionary<int, FieldItemRecord> FieldItems = [];

        [LoadRecord("OutpostBattleTable.json", "Id")]
        public readonly Dictionary<int, OutpostBattleRecord> OutpostBattle = [];

        [LoadRecord("JukeboxListTable.json", "Id")]
        public readonly Dictionary<int, JukeboxListRecord> jukeboxListDataRecords = [];

        [LoadRecord("JukeboxThemeTable.json", "Id")]
        public readonly Dictionary<int, JukeboxThemeRecord> jukeboxThemeDataRecords = [];

        [LoadRecord("GachaTypeTable.json", "Id")]
        public readonly Dictionary<int, GachaTypeRecord> gachaTypes = [];

        [LoadRecord("EventManagerTable.json", "Id")]
        public readonly Dictionary<int, EventManagerRecord> eventManagers = [];

        [LoadRecord("LiveWallpaperTable.json", "Id")]
        public readonly Dictionary<int, LiveWallpaperRecord> lwptablemgrs = [];

        [LoadRecord("AlbumResourceTable.json", "Id")]
        public readonly Dictionary<int, AlbumResourceRecord> albumResourceRecords = [];

        [LoadRecord("UserFrameTable.json", "Id")]
        public readonly Dictionary<int, UserFrameRecord> userFrameTable = [];

        [LoadRecord("ArchiveRecordManagerTable.json", "Id")]
        public readonly Dictionary<int, ArchiveRecordManagerRecord> archiveRecordManagerTable = [];

        [LoadRecord("ArchiveEventStoryTable.json", "Id")]
        public readonly Dictionary<int, ArchiveEventStoryRecord> archiveEventStoryRecords = [];

        [LoadRecord("ArchiveEventQuestTable.json", "Id")]
        public readonly Dictionary<int, ArchiveEventQuestRecord_Raw> archiveEventQuestRecords = [];

        [LoadRecord("ArchiveEventDungeonStageTable.json", "Id")]
        public readonly Dictionary<int, ArchiveEventDungeonStageRecord> archiveEventDungeonStageRecords = [];

        [LoadRecord("UserTitleTable.json", "Id")]
        public readonly Dictionary<int, UserTitleRecord> userTitleRecords = [];

        [LoadRecord("ArchiveMessengerConditionTable.json", "Id")]
        public readonly Dictionary<int, ArchiveMessengerConditionRecord> archiveMessengerConditionRecords = [];

        [LoadRecord("CharacterStatTable.json", "Id")]
        public readonly Dictionary<int, CharacterStatRecord> characterStatTable = [];

        [LoadRecord("SkillInfoTable.json", "Id")]
        public readonly Dictionary<int, SkillInfoRecord> skillInfoTable = [];

        [LoadRecord("CostTable.json", "Id")]
        public readonly Dictionary<int, CostRecord> costTable = [];

        [LoadRecord("MidasProductTable.json", "MidasProductIdProximabeta")]
        public readonly Dictionary<string, MidasProductRecord> mediasProductTable = [];

        [LoadRecord("TowerTable.json", "Id")]
        public readonly Dictionary<int, TowerRecord> towerTable = [];

        [LoadRecord("TriggerTable.json", "Id")]
        public readonly Dictionary<int, TriggerRecord> TriggerTable = [];

        [LoadRecord("InfraCoreGradeTable.json", "Id")]
        public readonly Dictionary<int, InfraCoreGradeRecord> InfracoreTable = [];

        [LoadRecord("AttractiveCounselCharacterTable.json", "NameCode")]
        public readonly Dictionary<int, AttractiveCounselCharacterRecord_Raw> AttractiveCounselCharacterTable = [];

        [LoadRecord("AttractiveLevelRewardTable.json", "Id")]
        public readonly Dictionary<int, AttractiveLevelRewardRecord> AttractiveLevelReward = [];

        [LoadRecord("AttractiveLevelTable.json", "Id")]
        public readonly Dictionary<int, AttractiveLevelRecord> AttractiveLevelTable = [];

        [LoadRecord("SubQuestTable.json", "Id")]
        public readonly Dictionary<int, SubQuestRecord> Subquests = [];

        [LoadRecord("MessengerDialogTable.json", "Id")]
        public readonly Dictionary<string, MessengerDialogRecord> Messages = [];

        [LoadRecord("MessengerConditionTriggerTable.json", "Id")]
        public readonly Dictionary<int, MessengerConditionTriggerRecord> MessageConditions = [];

        [LoadRecord("ScenarioRewardsTable.json", "ConditionId")]
        public readonly Dictionary<string, ScenarioRewardsRecord> ScenarioRewards = [];

        // Note: same data types are intentional
        [LoadRecord("ProductOfferTable.json", "Id")]
        public readonly Dictionary<int, ProductOfferRecord> ProductOffers = [];

        [LoadRecord("PopupPackageListTable.json", "Id")]
        public readonly Dictionary<int, PopupPackageListRecord> PopupPackages = [];

        [LoadRecord("InterceptNormalTable.json", "Id")]
        public readonly Dictionary<int, InterceptNormalRecord> InterceptNormal = [];

        [LoadRecord("InterceptSpecialTable.json", "Id")]
        public readonly Dictionary<int, InterceptSpecialRecord> InterceptSpecial = [];

        [LoadRecord("ConditionRewardTable.json", "Id")]
        public readonly Dictionary<int, ConditionRewardRecord> ConditionRewards = [];
        [LoadRecord("ItemConsumeTable.json", "Id")]
        public readonly Dictionary<int, ItemConsumeRecord> ConsumableItems = [];
        [LoadRecord("ItemRandomTable.json", "Id")]
        public readonly Dictionary<int, ItemRandomRecord> RandomItem = [];
        [LoadRecord("LostSectorTable.json", "Id")]
        public readonly Dictionary<int, LostSectorRecord> LostSector = [];
        [LoadRecord("LostSectorStageTable.json", "Id")]
        public readonly Dictionary<int, LostSectorStageRecord> LostSectorStages = [];
        [LoadRecord("ItemPieceTable.json", "Id")]
        public readonly Dictionary<int, ItemPieceRecord> PieceItems = [];
        [LoadRecord("GachaGradeProbTable.json", "Id")]
        public readonly Dictionary<int, GachaGradeProbRecord> GachaGradeProb = [];
        [LoadRecord("GachaListProbTable.json", "Id")]
        public readonly Dictionary<int, GachaListProbRecord> GachaListProb = [];
        [LoadRecord("RecycleResearchStatTable.json", "Id")]
        public readonly Dictionary<int, RecycleResearchStatRecord> RecycleResearchStats = [];
        [LoadRecord("RecycleResearchLevelTable.json", "Id")]
        public readonly Dictionary<int, RecycleResearchLevelRecord> RecycleResearchLevels = [];

        // Harmony Cube  Data Tables
        [LoadRecord("ItemHarmonyCubeTable.json", "Id")]
        public readonly Dictionary<int, ItemHarmonyCubeRecord> ItemHarmonyCubeTable = [];

        [LoadRecord("ItemHarmonyCubeLevelTable.json", "Id")]
        public readonly Dictionary<int, ItemHarmonyCubeLevelRecord> ItemHarmonyCubeLevelTable = [];

        // Favorite Item  Data Tables
        [LoadRecord("FavoriteItemTable.json", "Id")]
        public readonly Dictionary<int, FavoriteItemRecord> FavoriteItemTable = [];

        [LoadRecord("FavoriteItemExpTable.json", "Id")]
        public readonly Dictionary<int, FavoriteItemExpRecord> FavoriteItemExpTable = [];

        [LoadRecord("FavoriteItemLevelTable.json", "Id")]
        public readonly Dictionary<int, FavoriteItemLevelRecord> FavoriteItemLevelTable = [];

        [LoadRecord("FavoriteItemProbabilityTable.json", "Id")]
        public readonly Dictionary<int, FavoriteItemProbabilityRecord> FavoriteItemProbabilityTable = [];

        [LoadRecord("FavoriteItemQuestTable.json", "Id")]
        public readonly Dictionary<int, FavoriteItemQuestRecord> FavoriteItemQuestTable = [];

        [LoadRecord("FavoriteItemQuestStageTable.json", "Id")]
        public readonly Dictionary<int, FavoriteItemQuestStageRecord> FavoriteItemQuestStageTable = [];

        // Tables related to PlaySoda Arcade's event.

        [LoadRecord("EventPlaySodaManagerTable.json", "Id")]
        public readonly Dictionary<int, EventPlaySodaManagerRecord> EventPlaySodaManagerTable = [];

        [LoadRecord("EventPlaySodaStoryModeTable.json", "Id")]
        public readonly Dictionary<int, EventPlaySodaStoryModeRecord> EventPlaySodaStoryModeTable = [];

        [LoadRecord("EventPlaySodaChallengeModeTable.json", "Id")]
        public readonly Dictionary<int, EventPlaySodaChallengeModeRecord> EventPlaySodaChallengeModeTable = [];

        [LoadRecord("EventPlaySodaPointRewardTable.json", "Id")]
        public readonly Dictionary<int, EventPlaySodaPointRewardRecord> EventPlaySodaPointRewardTable = [];

        // Tables related to InTheMirror Arcade's event.

        [LoadRecord("EventMvgQuestTable.json", "Id")]
        public readonly Dictionary<int, EventMVGQuestRecord_Raw> EventMvgQuestTable = [];

        [LoadRecord("EventMvgShopTable.json", "Id")]
        public readonly Dictionary<int, EventMVGShopRecord_Raw> EventMvgShopTable = [];

        [LoadRecord("EventMVGMissionTable.json", "Id")]
        public readonly Dictionary<int, EventMVGMissionRecord_Raw> EventMvgMissionTable = [];

        [LoadRecord("EquipmentOptionTable.json", "Id")]
        public readonly Dictionary<int, EquipmentOptionRecord> EquipmentOptionTable = [];

        [LoadRecord("EquipmentOptionCostTable.json", "Id")]
        public readonly Dictionary<int, EquipmentOptionCostRecord> EquipmentOptionCostTable = [];

        [LoadRecord("ItemEquipCorpSettingTable.json", "Id")]
        public readonly Dictionary<int, ItemEquipCorpSettingRecord> ItemEquipCorpSettingTable = [];

        [LoadRecord("LobbyPrivateBannerTable.json", "Id")]
        public readonly Dictionary<int, LobbyPrivateBannerRecord> LobbyPrivateBannerTable = [];

        [LoadRecord("LoginEventTable.json", "Id")]
        public readonly Dictionary<int, LoginEventRecord> LoginEventTable = [];
        
        // Event Dungeon data Table
        [LoadRecord("EventDungeonTable.json", "Id")]
        public readonly Dictionary<int, EventDungeonRecord> EventDungeonTable = [];
        [LoadRecord("EventDungeonStageTable.json", "Id")]
        public readonly Dictionary<int, EventDungeonStageRecord> EventDungeonStageTable = [];
        [LoadRecord("EventDungeonSpotBattleTable.json", "Id")]
        public readonly Dictionary<int, EventDungeonSpotBattleRecord> EventDungeonSpotBattleTable = [];
        [LoadRecord("EventDungeonDifficultTable.json", "Id")]
        public readonly Dictionary<int, EventDungeonDifficultRecord> EventDungeonDifficultTable = [];
        
        // Pass Data Tables
        [LoadRecord("PassManagerTable.json", "Id")]
        public readonly Dictionary<int, PassManagerRecord> PassManagerTable = [];
        [LoadRecord("EventPassManagerTable.json", "Id")]
        public readonly Dictionary<int, EventPassManagerRecord> EventPassManagerTable = [];
        [LoadRecord("SeasonPassTable.json", "Id")]
        public readonly Dictionary<int, SeasonPassRecord> SeasonPassTable = [];
        [LoadRecord("PassMissionTable.json", "Id")]
        public readonly Dictionary<int, PassMissionRecord> PassMissionTable = [];

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

        public GameData(string mpkFilePath)
        {
            if (!File.Exists(mpkFilePath)) throw new ArgumentException("Static data file must exist", nameof(mpkFilePath));

            // disable warnings
            ZipStream = new();

            byte[] rawBytes2 = File.ReadAllBytes(mpkFilePath);
            MpkHash = SHA256.HashData(rawBytes2);
            MpkSize = rawBytes2.Length;

            LoadGameData(mpkFilePath, GameConfig.Root.StaticDataMpk);
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
            string? targetFile2 = await AssetDownloadUtil.DownloadOrGetFileAsync(GameConfig.Root.StaticDataMpk.Url, CancellationToken.None) ?? throw new Exception("static data download fail");
            _instance = new(targetFile2);
        }
        #endregion

        public async Task<X[]> LoadZip<X>(string entry, IProgress<double> bar) where X : new()
        {
            try
            {
                entry = entry.Replace(".json", ".mpk");

                ZipEntry fileEntry = MainZip.GetEntry(entry);
                if (fileEntry == null)
                {
                    Logging.WriteLine(entry + " does not exist in static data", LogType.Error);
                    return [];
                }

                Stream stream = MainZip.GetInputStream(fileEntry);
                X[] deserializedObject = await MemoryPackSerializer.DeserializeAsync<X[]>(stream) ?? throw new Exception("failed to parse " + entry);

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
                if (item.Name.StartsWith("FieldMapData_") && item.Name != "FieldMapData_EventMap.mpk")
                {
                    FieldMapRecord[] x = await LoadZip<FieldMapRecord>(item.Name, progress);

                    foreach (FieldMapRecord map in x)
                    {
                        MapData.Add(map.Id, map);
                    }
                }
            }




            // sanity checks
            if (QuestDataRecords.Count == 0) throw new Exception("QuestDataRecords should not be empty");
        }

        public MainQuestRecord? GetMainQuestForStageClearCondition(int stage)
        {
            if (QuestDataRecords.Count == 0) throw new Exception("QuestDataRecords should not be empty");
            foreach (KeyValuePair<int, MainQuestRecord> item in QuestDataRecords)
            {
                if (item.Value.ConditionId[0].ConditionId == stage)
                {
                    return item.Value;
                }
            }

            return null;
        }
        public MainQuestRecord? GetMainQuestByTableId(int tId)
        {
            return QuestDataRecords[tId];
        }
        public CampaignStageRecord? GetStageData(int stage)
        {
            return StageDataRecords[stage];
        }
        public RewardRecord? GetRewardTableEntry(int rewardId)
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
                    prevLevel = item.Level;
                    prevValue = item.Exp;
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

                if (targetLevel == item.Level)
                {
                    return item.Exp;
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
                    if (difficulty == "Normal" && item.Value.Chapter == chapterNum)
                    {
                        return item.Value.FieldId;
                    }
                    else if (difficulty == "Hard" && item.Value.Chapter == chapterNum)
                    {
                        return item.Value.HardFieldId;
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
                if (item.Value.FieldId == field)
                {
                    return item.Value.Chapter;
                }
            }

            return -1;
        }
        public IEnumerable<int> GetAllCostumes()
        {
            foreach (KeyValuePair<int, CharacterCostumeRecord> item in CharacterCostumeTable)
            {
                yield return item.Value.Id;
            }
        }

        internal ContentsTutorialRecord GetTutorialDataById(int TableId)
        {
            return TutorialTable[TableId];
        }

        public ItemSubType GetItemSubType(int itemType)
        {
            // Check if it's an equipment item
            if (ItemEquipTable.TryGetValue(itemType, out ItemEquipRecord? equipRecord))
            {
                return equipRecord.ItemSubType;
            }

            // Check if it's a harmony cube item
            if (ItemHarmonyCubeTable.TryGetValue(itemType, out ItemHarmonyCubeRecord? harmonyCubeRecord))
            {
                return harmonyCubeRecord.ItemSubType;
            }

            // Return null if item type not found
            return ItemSubType.None;
        }

        internal IEnumerable<int> GetStageIdsForChapter(int chapterNumber, bool normal)
        {
            ChapterMod mod = normal ? ChapterMod.Normal : ChapterMod.Hard;
            foreach (KeyValuePair<int, CampaignStageRecord> item in StageDataRecords)
            {
                CampaignStageRecord data = item.Value;

                int chVal = data.ChapterId - 1;

                if (chapterNumber == chVal && data.ChapterMod == mod && data.StageType == StageType.Main)
                {
                    yield return data.Id;
                }
            }
        }

        public Dictionary<int, CharacterLevelRecord> GetCharacterLevelUpData()
        {
            return LevelData;
        }

        public TacticAcademyFunctionRecord GetTacticAcademyLesson(int lessonId)
        {
            return TacticAcademyLessons[lessonId];
        }

        public IEnumerable<string> GetScenarioStageIdsForChapter(int chapterNumber)
        {
            return albumResourceRecords.Values.Where(record => record.TargetChapter == chapterNumber && !string.IsNullOrEmpty(record.ScenarioGroupId)).Select(record => record.ScenarioGroupId);
        }
        public bool IsValIdScenarioStage(string scenarioGroupId, int targetChapter, int targetStage)
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
                return false; // If it doesn't have at least 4 parts, it's not a valId stage
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
                return data.HardFieldId;
            else return data.FieldId;
        }

        internal int GetConditionReward(int groupId, long damage)
        {
            IEnumerable<KeyValuePair<int, ConditionRewardRecord>> results = ConditionRewards.Where(x => x.Value.Group == groupId && x.Value.ValueMin <= damage && (x.Value.ValueMax == 0 || x.Value.ValueMax >= damage));
            if (results.Any())
                return results.FirstOrDefault().Value.RewardId;
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

    public class DataTable<T>
    {
        public string version { get; set; } = "";
        public List<T> records { get; set; } = [];
    }
}