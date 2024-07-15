using nksrv.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using Swan.Parsers;
using Newtonsoft.Json;
using System.Drawing;
using Google.Protobuf.WellKnownTypes;

namespace nksrv.StaticInfo
{
    /// <summary>
    /// "Static data" which is what the game calls it, contains data such as map info, characters, quests, rewards and a lot more.
    /// </summary>
    public class StaticDataParser
    {
        // These fields were extracted from the game.
        public static byte[] PresharedKey = [0xCB, 0xC2, 0x1C, 0x6F, 0xF3, 0xF5, 0x07, 0xF5, 0x05, 0xBA, 0xCA, 0xD4, 0x98, 0x28, 0x84, 0x1F, 0xF0, 0xD1, 0x38, 0xC7, 0x61, 0xDF, 0xD6, 0xE6, 0x64, 0x9A, 0x85, 0x13, 0x3E, 0x1A, 0x6A, 0x0C, 0x68, 0x0E, 0x2B, 0xC4, 0xDF, 0x72, 0xF8, 0xC6, 0x55, 0xE4, 0x7B, 0x14, 0x36, 0x18, 0x3B, 0xA7, 0xD1, 0x20, 0x81, 0x22, 0xD1, 0xA9, 0x18, 0x84, 0x65, 0x13, 0x0B, 0xED, 0xA3, 0x00, 0xE5, 0xD9];
        public static RSAParameters RSAParameters = new RSAParameters()
        {
            Exponent = [0x01, 0x00, 0x01],
            Modulus = [0x89, 0xD6, 0x66, 0x00, 0x7D, 0xFC, 0x7D, 0xCE, 0x83, 0xA6, 0x62, 0xE3, 0x1A, 0x5E, 0x9A, 0x53, 0xC7, 0x8A, 0x27, 0xF3, 0x67, 0xC1, 0xF3, 0xD4, 0x37, 0xFE, 0x50, 0x6D, 0x38, 0x45, 0xDF, 0x7E, 0x73, 0x5C, 0xF4, 0x9D, 0x40, 0x4C, 0x8C, 0x63, 0x21, 0x97, 0xDF, 0x46, 0xFF, 0xB2, 0x0D, 0x0E, 0xDB, 0xB2, 0x72, 0xB4, 0xA8, 0x42, 0xCD, 0xEE, 0x48, 0x06, 0x74, 0x4F, 0xE9, 0x56, 0x6E, 0x9A, 0xB1, 0x60, 0x18, 0xBC, 0x86, 0x0B, 0xB6, 0x32, 0xA7, 0x51, 0x00, 0x85, 0x7B, 0xC8, 0x72, 0xCE, 0x53, 0x71, 0x3F, 0x64, 0xC2, 0x25, 0x58, 0xEF, 0xB0, 0xC9, 0x1D, 0xE3, 0xB3, 0x8E, 0xFC, 0x55, 0xCF, 0x8B, 0x02, 0xA5, 0xC8, 0x1E, 0xA7, 0x0E, 0x26, 0x59, 0xA8, 0x33, 0xA5, 0xF1, 0x11, 0xDB, 0xCB, 0xD3, 0xA7, 0x1F, 0xB1, 0xC6, 0x10, 0x39, 0xC8, 0x31, 0x1D, 0x60, 0xDB, 0x0D, 0xA4, 0x13, 0x4B, 0x2B, 0x0E, 0xF3, 0x6F, 0x69, 0xCB, 0xA8, 0x62, 0x03, 0x69, 0xE6, 0x95, 0x6B, 0x8D, 0x11, 0xF6, 0xAF, 0xD9, 0xC2, 0x27, 0x3A, 0x32, 0x12, 0x05, 0xC3, 0xB1, 0xE2, 0x81, 0x4B, 0x40, 0xF8, 0x8B, 0x8D, 0xBA, 0x1F, 0x55, 0x60, 0x2C, 0x09, 0xC6, 0xED, 0x73, 0x96, 0x32, 0xAF, 0x5F, 0xEE, 0x8F, 0xEB, 0x5B, 0x93, 0xCF, 0x73, 0x13, 0x15, 0x6B, 0x92, 0x7B, 0x27, 0x0A, 0x13, 0xF0, 0x03, 0x4D, 0x6F, 0x5E, 0x40, 0x7B, 0x9B, 0xD5, 0xCE, 0xFC, 0x04, 0x97, 0x7E, 0xAA, 0xA3, 0x53, 0x2A, 0xCF, 0xD2, 0xD5, 0xCF, 0x52, 0xB2, 0x40, 0x61, 0x28, 0xB1, 0xA6, 0xF6, 0x78, 0xFB, 0x69, 0x9A, 0x85, 0xD6, 0xB9, 0x13, 0x14, 0x6D, 0xC4, 0x25, 0x36, 0x17, 0xDB, 0x54, 0x0C, 0xD8, 0x77, 0x80, 0x9A, 0x00, 0x62, 0x83, 0xDD, 0xB0, 0x06, 0x64, 0xD0, 0x81, 0x5B, 0x0D, 0x23, 0x9E, 0x88, 0xBD],
            DP = null
        };

        // Fields
        private static StaticDataParser? _instance;
        public static StaticDataParser Instance
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
        private JArray questDataRecords;
        private JArray stageDataRecords;
        private JArray rewardDataRecords;
        private JArray userExpDataRecords;
        private JArray chapterCampaignData;
        private JArray characterCostumeTable;
        private JArray characterTable;
        private JArray tutorialTable;
        private JArray itemEquipTable;
        private Dictionary<int, CharacterLevelData> LevelData = [];
        private Dictionary<int, TacticAcademyLessonRecord> TacticAcademyLessons = [];

        public byte[] Sha256Hash;
        public int Size;

        static async Task<StaticDataParser> BuildAsync()
        {
            Logger.Info("Loading static data");
            await Load();

            Logger.Info("Parsing static data");
            await Instance.Parse();

            return Instance;
        }

        public StaticDataParser(string filePath)
        {
            if (!File.Exists(filePath)) throw new ArgumentException("Static data file must exist", nameof(filePath));

            // disable warnings
            questDataRecords = new();
            stageDataRecords = new();
            rewardDataRecords = new();
            userExpDataRecords = new();
            chapterCampaignData = new();
            characterCostumeTable = new();
            characterTable = new();
            ZipStream = new();
            tutorialTable = new();
            itemEquipTable = new();

            var rawBytes = File.ReadAllBytes(filePath);
            Sha256Hash = SHA256.HashData(rawBytes);
            Size = rawBytes.Length;

            DecryptStaticDataAndLoadZip(filePath);
            if (MainZip == null) throw new Exception("failed to read zip file");
        }
        #region Decryption
        private void DecryptStaticDataAndLoadZip(string file)
        {
            using var fileStream = File.Open(file, FileMode.Open, FileAccess.Read);

            var keyDecryptor = new Rfc2898DeriveBytes(PresharedKey, GameConfig.Root.StaticData.GetSalt2Bytes(), 10000, HashAlgorithmName.SHA256);
            var key2 = keyDecryptor.GetBytes(32);

            byte[] decryptionKey = key2[0..16];
            byte[] iv = key2[16..32];
            var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Key = decryptionKey;
            aes.IV = iv;
            var transform = aes.CreateDecryptor();

            // Decryption layer 1
            using CryptoStream stream = new CryptoStream(fileStream, transform, CryptoStreamMode.Read);

            using MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);

            var bytes = ms.ToArray();

            // Decryption of layer 2
            var zip = new ZipFile(ms, false);

            var signEntry = zip.GetEntry("sign");
            if (signEntry == null) throw new Exception("Sign entry not found in decrypted static data pack");
            var dataEntry = zip.GetEntry("data");
            if (dataEntry == null) throw new Exception("Data entry not found in decrypted static data pack");

            var signStream = zip.GetInputStream(signEntry);
            var dataStream = zip.GetInputStream(dataEntry);

            using MemoryStream signMs = new MemoryStream();
            signStream.CopyTo(signMs);

            using MemoryStream dataMs = new MemoryStream();
            dataStream.CopyTo(dataMs);
            dataMs.Position = 0;

            var rsa = RSA.Create(RSAParameters);
            if (!rsa.VerifyData(dataMs, signMs.ToArray(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                throw new Exception("failed to decrypt static data (round 2)");

            dataMs.Position = 0;

            // Decryption of layer 3
            var keyDecryptor2 = new Rfc2898DeriveBytes(PresharedKey, GameConfig.Root.StaticData.GetSalt1Bytes(), 10000, HashAlgorithmName.SHA256);
            var key3 = keyDecryptor2.GetBytes(32);

            byte[] decryptionKey2 = key3[0..16];
            byte[] iv2 = key3[16..32];

            ZipStream = new MemoryStream();
            AesCtrTransform(decryptionKey2, iv2, dataMs, ZipStream);

            ZipStream.Position = 0;

            MainZip = new ZipFile(ZipStream, false);
        }

        public static void AesCtrTransform(byte[] key, byte[] salt, Stream inputStream, Stream outputStream)
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

        private async Task<JArray> LoadZip(string entry)
        {
            var mainQuestData = MainZip.GetEntry(entry);
            if (mainQuestData == null) throw new Exception(entry + " does not exist in static data");

            using StreamReader mainQuestReader = new StreamReader(MainZip.GetInputStream(mainQuestData));
            var mainQuestDataString = await mainQuestReader.ReadToEndAsync();


            var questdata = JObject.Parse(mainQuestDataString);
            if (questdata == null) throw new Exception("failed to parse " + entry);

            var records = (JArray?)questdata["records"];
            if (records == null) throw new Exception(entry + " is missing records element");

            return records;
        }
        public async Task Parse()
        {
            questDataRecords = await LoadZip("MainQuestTable.json");
            stageDataRecords = await LoadZip("CampaignStageTable.json");
            rewardDataRecords = await LoadZip("RewardTable.json");
            userExpDataRecords = await LoadZip("UserExpTable.json");
            chapterCampaignData = await LoadZip("CampaignChapterTable.json");
            characterCostumeTable = await LoadZip("CharacterCostumeTable.json");
            characterTable = await LoadZip("CharacterTable.json");
            tutorialTable = await LoadZip("ContentsTutorialTable.json");
            itemEquipTable = await LoadZip("ItemEquipTable.json");
            var characterLevelTable = await LoadZip("CharacterLevelTable.json");

            foreach (JToken item in characterLevelTable)
            {
                var obj = item.ToObject<CharacterLevelData>();
                if (obj != null)
                    LevelData.Add(obj.level, obj);
                else
                    Logger.Warn("failed to read character level table entry");
            }

            var tacticLessonTable = await LoadZip("TacticAcademyFunctionTable.json");

            foreach (JToken item in tacticLessonTable)
            {
                var idRaw = item["id"];
                var groupidRaw = item["group_id"];
                var currencyIdRaw = item["currency_id"];
                var currencyValueRaw = item["currency_value"];

                if (idRaw == null) throw new InvalidDataException();
                if (groupidRaw == null) throw new InvalidDataException();
                if (currencyIdRaw == null) throw new InvalidDataException();
                if (currencyValueRaw == null) throw new InvalidDataException();

                var id = idRaw.ToObject<int>();
                var currencyId = currencyIdRaw.ToObject<int>();
                var currencyValue = currencyValueRaw.ToObject<int>();
                var groupid = groupidRaw.ToObject<int>();

                var fullId = int.Parse(groupid.ToString() + id.ToString());
                TacticAcademyLessons.Add(id, new TacticAcademyLessonRecord() { CurrencyId = (CurrencyType)currencyId, CurrencyValue = currencyValue, GroupId = groupid, Id = id });
            }
        }

        public MainQuestCompletionData? GetMainQuestForStageClearCondition(int stage)
        {
            foreach (JObject item in questDataRecords)
            {
                var id = item["condition_id"];
                if (id == null) throw new Exception("expected condition_id field in quest data");

                int value = id.ToObject<int>();
                if (value == stage)
                {
                    MainQuestCompletionData? data = item.ToObject<MainQuestCompletionData>();
                    if (data == null) throw new Exception("failed to deserialize main quest data item");
                    return data;
                }
            }

            return null;
        }
        public MainQuestCompletionData? GetMainQuestByTableId(int tid)
        {
            foreach (JObject item in questDataRecords)
            {
                var id = item["id"];
                if (id == null) throw new Exception("expected condition_id field in quest data");

                int value = id.ToObject<int>();
                if (value == tid)
                {
                    MainQuestCompletionData? data = item.ToObject<MainQuestCompletionData>();
                    if (data == null) throw new Exception("failed to deserialize main quest data item");
                    return data;
                }
            }

            return null;
        }
        public CampaignStageRecord? GetStageData(int stage)
        {
            foreach (JObject item in stageDataRecords)
            {
                var id = item["id"];
                if (id == null) throw new Exception("expected id field in campaign data");

                int value = id.ToObject<int>();
                if (value == stage)
                {
                    CampaignStageRecord? data = JsonConvert.DeserializeObject<CampaignStageRecord>(item.ToString());
                    if (data == null) throw new Exception("failed to deserialize stage data");
                    return data;
                }
            }

            return null;
        }
        public RewardTableRecord? GetRewardTableEntry(int rewardId)
        {
            foreach (JObject item in rewardDataRecords)
            {
                var id = item["id"];
                if (id == null) throw new Exception("expected id field in reward data");

                int value = id.ToObject<int>();
                if (value == rewardId)
                {
                    RewardTableRecord? data = JsonConvert.DeserializeObject<RewardTableRecord>(item.ToString());
                    if (data == null) throw new Exception("failed to deserialize reward data");
                    return data;
                }
            }

            return null;
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
        public int GetNormalChapterNumberFromFieldName(string field)
        {
            foreach (JObject item in chapterCampaignData)
            {
                var id = item["field_id"];
                if (id == null)
                {
                    throw new Exception("expected id field in reward data");
                }

                string? value = id.ToObject<string>();
                if (value == field)
                {
                    var chapter = item["chapter"];
                    if (chapter == null)
                    {
                        throw new Exception("expected id field in reward data");
                    }

                    return chapter.ToObject<int>();
                }
            }

            return -1;
        }

        public IEnumerable<int> GetAllCharacterTids()
        {
            foreach (JObject item in characterTable)
            {
                var id = item["id"];
                if (id == null) throw new Exception("expected id field in reward data");

                int value = id.ToObject<int>();
                yield return value;
            }
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
            foreach (JObject item in tutorialTable)
            {
                var id = item["id"];
                if (id == null)
                {
                    throw new Exception("expected id field in reward data");
                }

                int idValue = id.ToObject<int>();
                if (idValue == TableId)
                {
                    ClearedTutorialData? data = item.ToObject<ClearedTutorialData>();
                    if (data == null) throw new Exception("failed to deserialize reward data");
                    return data;
                }
            }

            throw new Exception("tutorial not found: " + TableId);
        }

        public string? GetItemSubType(int itemType)
        {
            foreach (JObject item in itemEquipTable)
            {
                var id = item["id"];
                if (id == null) throw new Exception("expected id field in reward data");

                int? idValue = id.ToObject<int>();
                if (idValue == itemType)
                {
                    var subtype = item["item_sub_type"];
                    if (subtype == null)
                    {
                        throw new Exception("expected item_sub_type field in item equip data");
                    }

                    return subtype.ToObject<string>();
                }
            }

            return null;
        }

        internal IEnumerable<int> GetStageIdsForChapter(int chapterNumber, bool normal)
        {
            string mod = normal ? "Normal" : "Hard";
            foreach (JObject item in stageDataRecords)
            {
                CampaignStageRecord? data = item.ToObject<CampaignStageRecord>();
                if (data == null) throw new Exception("failed to deserialize stage data");

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
    }
}
