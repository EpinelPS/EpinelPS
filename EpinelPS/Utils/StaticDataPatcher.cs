using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using EpinelPS.Data;
using MemoryPack;
using Newtonsoft.Json;

namespace EpinelPS.Utils;

public static class StaticDataPatcher
{
    private class GameCommon
    {
        public byte[] SmallNumber { get; set; } = [];
    }

    private static readonly HttpClient ProbeClient = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.All
    })
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

    private static readonly string[] KnownLanguageTags =
    [
        "_en", "_ja", "_ko", "_zh-tw", "_zh-cn", "_de", "_th", "_fr"
    ];

    private static readonly string[] SupportedLanguages =
    [
        "en", "ja", "ko", "zh-TW", "zh-CN", "de", "th", "fr"
    ];

    /// <summary>
    /// Hybrid Fast-Path Auto-Detection:
    /// Scans ScenarioMovieTable.mpk for unlocalized video cutscene links, probes the official CDN
    /// for 404s with localized sibling matches, and dynamically patches the table.
    /// Fast-path marker (.patched) ensures 0 ms overhead on all subsequent server boots.
    /// </summary>
    public static async Task<bool> TryPatchAsync(string packPath, StaticData data)
    {
        if (!File.Exists(packPath))
        {
            return false;
        }

        string markerPath = packPath + ".patched";
        if (File.Exists(markerPath))
        {
            // Fast-path: already processed and verified, 0 ms overhead
            return false;
        }

        try
        {
            string commonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gamecommon.json");
            if (!File.Exists(commonPath))
            {
                Logging.WriteLine("[StaticDataPatcher] gamecommon.json not found; skipping patch check.", LogType.Warning);
                return false;
            }

            var common = JsonConvert.DeserializeObject<GameCommon>(File.ReadAllText(commonPath));
            if (common == null || common.SmallNumber.Length == 0)
            {
                return false;
            }

            byte[] fileBytes = File.ReadAllBytes(packPath);

            // Step 1: Derive key2 for outer AES-CBC layer
            byte[] key2 = Rfc2898DeriveBytes.Pbkdf2(
                common.SmallNumber,
                data.GetSalt2Bytes(),
                10000,
                HashAlgorithmName.SHA256,
                32);

            byte[] decryptedOuter;
            using (Aes aesOuter = Aes.Create())
            {
                aesOuter.Key = key2[..16];
                aesOuter.IV = key2[16..];
                aesOuter.Mode = CipherMode.CBC;
                aesOuter.Padding = PaddingMode.PKCS7;

                using MemoryStream msIn = new(fileBytes);
                using CryptoStream decryptStream = new(msIn, aesOuter.CreateDecryptor(), CryptoStreamMode.Read);
                using MemoryStream msOut = new();
                decryptStream.CopyTo(msOut);
                decryptedOuter = msOut.ToArray();
            }

            // Step 2: Read outer zip to get "sign" and "data"
            byte[] signBytes;
            byte[] dataBytes;
            using (MemoryStream outerZipMs = new(decryptedOuter))
            using (ZipArchive outerZip = new(outerZipMs, ZipArchiveMode.Read))
            {
                ZipArchiveEntry? signEntry = outerZip.GetEntry("sign");
                ZipArchiveEntry? dataEntry = outerZip.GetEntry("data");
                if (signEntry == null || dataEntry == null)
                {
                    return false;
                }

                using MemoryStream signMs = new();
                using (Stream s = signEntry.Open()) s.CopyTo(signMs);
                signBytes = signMs.ToArray();

                using MemoryStream dataMs = new();
                using (Stream s = dataEntry.Open()) s.CopyTo(dataMs);
                dataBytes = dataMs.ToArray();
            }

            // Step 3: Decrypt inner data using AES-CTR (key3)
            byte[] key3 = Rfc2898DeriveBytes.Pbkdf2(
                common.SmallNumber,
                data.GetSalt1Bytes(),
                10000,
                HashAlgorithmName.SHA256,
                32);

            byte[] innerZipBytes;
            using (MemoryStream dataInMs = new(dataBytes))
            using (MemoryStream innerZipOutMs = new())
            {
                GameData.DoTransformation(key3[..16], key3[16..], dataInMs, innerZipOutMs);
                innerZipBytes = innerZipOutMs.ToArray();
            }

            // Step 4: Extract ScenarioMovieTable.mpk and deserialize records
            ScenarioMovieRecord[]? movieRecords = null;
            using (MemoryStream innerZipMs = new(innerZipBytes))
            using (ZipArchive innerZip = new(innerZipMs, ZipArchiveMode.Read))
            {
                ZipArchiveEntry? movieEntry = innerZip.GetEntry("ScenarioMovieTable.mpk");
                if (movieEntry == null)
                {
                    return false;
                }

                using MemoryStream movieMs = new();
                using (Stream s = movieEntry.Open()) s.CopyTo(movieMs);
                movieRecords = MemoryPackSerializer.Deserialize<ScenarioMovieRecord[]>(movieMs.ToArray());
            }

            if (movieRecords == null || movieRecords.Length == 0)
            {
                return false;
            }

            // Step 5: Extract unlocalized candidate URLs (e.g. without _en, _ja, _ko tags)
            List<string> candidateUrls = ExtractUnlocalizedUrls(movieRecords);
            if (candidateUrls.Count == 0)
            {
                await File.WriteAllTextAsync(markerPath, "ok");
                return false;
            }

            // Step 6: Parallel probe candidate URLs via HTTP HEAD to detect 404s with localized CDN siblings
            Dictionary<string, Dictionary<string, string>> defectiveMap = await DetectDefectiveUrlsAsync(candidateUrls);
            if (defectiveMap.Count == 0)
            {
                await File.WriteAllTextAsync(markerPath, "ok");
                return false;
            }

            Logging.WriteLine($"[StaticDataPatcher] Detected {defectiveMap.Count} defective cutscene URL(s) in StaticData.pack. Applying HFP dynamic localization...", LogType.Info);

            // Step 7: Apply localized URLs to ScenarioMovieRecord array and serialize back
            byte[] patchedMovieBytes = PatchScenarioMovieTable(movieRecords, defectiveMap);

            // Step 8: Repack inner zip with updated ScenarioMovieTable.mpk
            byte[] newInnerZipBytes;
            using (MemoryStream innerZipMs = new(innerZipBytes))
            using (ZipArchive innerZip = new(innerZipMs, ZipArchiveMode.Read))
            using (MemoryStream newInnerMs = new())
            {
                using (ZipArchive newInnerZip = new(newInnerMs, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (ZipArchiveEntry entry in innerZip.Entries)
                    {
                        ZipArchiveEntry newEntry = newInnerZip.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                        using Stream src = entry.Open();
                        using Stream dst = newEntry.Open();

                        if (entry.FullName == "ScenarioMovieTable.mpk")
                        {
                            dst.Write(patchedMovieBytes, 0, patchedMovieBytes.Length);
                        }
                        else
                        {
                            src.CopyTo(dst);
                        }
                    }
                }
                newInnerZipBytes = newInnerMs.ToArray();
            }

            // Step 9: Encrypt inner zip with AES-CTR
            byte[] newEncryptedData;
            using (MemoryStream newInnerStream = new(newInnerZipBytes))
            using (MemoryStream encryptedDataMs = new())
            {
                GameData.DoTransformation(key3[..16], key3[16..], newInnerStream, encryptedDataMs);
                newEncryptedData = encryptedDataMs.ToArray();
            }

            // Step 10: Build new outer zip (uncompressed) containing original sign and new data
            byte[] newOuterZipBytes;
            using (MemoryStream newOuterMs = new())
            {
                using (ZipArchive newOuterZip = new(newOuterMs, ZipArchiveMode.Create, leaveOpen: true))
                {
                    ZipArchiveEntry newSignEntry = newOuterZip.CreateEntry("sign", CompressionLevel.NoCompression);
                    using (Stream dst = newSignEntry.Open())
                    {
                        dst.Write(signBytes, 0, signBytes.Length);
                    }

                    ZipArchiveEntry newDataEntry = newOuterZip.CreateEntry("data", CompressionLevel.NoCompression);
                    using (Stream dst = newDataEntry.Open())
                    {
                        dst.Write(newEncryptedData, 0, newEncryptedData.Length);
                    }
                }
                newOuterZipBytes = newOuterMs.ToArray();
            }

            // Step 11: Encrypt outer zip using AES-CBC
            byte[] newPackBytes;
            using (Aes aesOuter = Aes.Create())
            {
                aesOuter.Key = key2[..16];
                aesOuter.IV = key2[16..];
                aesOuter.Mode = CipherMode.CBC;
                aesOuter.Padding = PaddingMode.PKCS7;

                using MemoryStream msIn = new(newOuterZipBytes);
                using MemoryStream msOut = new();
                using (CryptoStream encryptStream = new(msOut, aesOuter.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    msIn.CopyTo(encryptStream);
                    encryptStream.FlushFinalBlock();
                }
                newPackBytes = msOut.ToArray();
            }

            // Step 12: Backup original and write patched file
            string backupPath = packPath + ".bak";
            if (!File.Exists(backupPath))
            {
                File.Copy(packPath, backupPath);
            }

            await File.WriteAllBytesAsync(packPath, newPackBytes);
            await File.WriteAllTextAsync(markerPath, "patched");
            Logging.WriteLine($"[StaticDataPatcher] Successfully patched StaticData.pack ({newPackBytes.Length} bytes). Video cutscenes are now localized and viewable.", LogType.Info);

            return true;
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"[StaticDataPatcher] Failed to patch StaticData.pack: {ex.Message}", LogType.Error);
            return false;
        }
    }

    private static List<string> ExtractUnlocalizedUrls(ScenarioMovieRecord[] records)
    {
        HashSet<string> unlocalized = [];
        foreach (ScenarioMovieRecord record in records)
        {
            if (string.IsNullOrEmpty(record.MovieLink))
            {
                continue;
            }

            if (IsUnlocalizedUrl(record.MovieLink))
            {
                unlocalized.Add(record.MovieLink);
            }
        }
        return [.. unlocalized];
    }

    private static bool IsUnlocalizedUrl(string url)
    {
        if (!url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string withoutExt = url[..^4].ToLowerInvariant();
        foreach (string tag in KnownLanguageTags)
        {
            if (withoutExt.EndsWith(tag, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<Dictionary<string, Dictionary<string, string>>> DetectDefectiveUrlsAsync(List<string> candidateUrls)
    {
        Dictionary<string, Dictionary<string, string>> defectiveMap = [];

        var probeTasks = candidateUrls.Select(async url =>
        {
            try
            {
                using HttpRequestMessage headReq = new(HttpMethod.Head, url);
                using HttpResponseMessage headResp = await ProbeClient.SendAsync(headReq);
                if (headResp.StatusCode == HttpStatusCode.NotFound)
                {
                    // Base URL returns 404 on CDN; check if localized siblings exist
                    string withoutExt = url[..^4];
                    string[] probeLangs = ["en", "ja", "ko"];

                    var variantTasks = probeLangs.Select(async lang =>
                    {
                        string variantUrl = $"{withoutExt}_{lang}.mp4";
                        try
                        {
                            using HttpRequestMessage vReq = new(HttpMethod.Head, variantUrl);
                            using HttpResponseMessage vResp = await ProbeClient.SendAsync(vReq);
                            return (lang, variantUrl, exists: vResp.StatusCode == HttpStatusCode.OK);
                        }
                        catch
                        {
                            return (lang, variantUrl, exists: false);
                        }
                    }).ToArray();

                    var variantResults = await Task.WhenAll(variantTasks);
                    var existingVariants = variantResults.Where(x => x.exists).ToDictionary(x => x.lang, x => x.variantUrl);

                    if (existingVariants.Count > 0)
                    {
                        string fallbackUrl = existingVariants.TryGetValue("en", out var enUrl)
                            ? enUrl
                            : existingVariants.Values.First();

                        Dictionary<string, string> langMap = [];
                        foreach (string lang in SupportedLanguages)
                        {
                            langMap[lang] = existingVariants.TryGetValue(lang, out var specificUrl)
                                ? specificUrl
                                : fallbackUrl;
                        }

                        return (url, (Dictionary<string, string>?)langMap);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"[StaticDataPatcher] Probe skipped for {url}: {ex.Message}", LogType.Warning);
            }

            return (url, null);
        }).ToArray();

        var probeResults = await Task.WhenAll(probeTasks);
        foreach (var (url, langMap) in probeResults)
        {
            if (langMap != null)
            {
                defectiveMap[url] = langMap;
            }
        }

        // Offline safety fallback: if probe was offline, ensure known defects are handled
        if (defectiveMap.Count == 0)
        {
            foreach (string url in candidateUrls)
            {
                if (url.EndsWith("coinrushshowdown/coinrushshowdown.mp4", StringComparison.OrdinalIgnoreCase))
                {
                    string withoutExt = url[..^4];
                    Dictionary<string, string> langMap = [];
                    foreach (string lang in SupportedLanguages)
                    {
                        string targetLang = (lang is "en" or "ja" or "ko") ? lang : "en";
                        langMap[lang] = $"{withoutExt}_{targetLang}.mp4";
                    }
                    defectiveMap[url] = langMap;
                }
            }
        }

        return defectiveMap;
    }

    private static byte[] PatchScenarioMovieTable(ScenarioMovieRecord[] records, Dictionary<string, Dictionary<string, string>> defectiveMap)
    {
        foreach (ScenarioMovieRecord record in records)
        {
            if (record.MovieLink != null && defectiveMap.TryGetValue(record.MovieLink, out var langMap))
            {
                string lang = record.Language ?? "en";
                if (langMap.TryGetValue(lang, out var newUrl))
                {
                    record.MovieLink = newUrl;
                }
            }
        }

        return MemoryPackSerializer.Serialize(records);
    }
}
