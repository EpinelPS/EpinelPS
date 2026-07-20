using Microsoft.Data.Sqlite;
using EpinelPS.Utils;

namespace EpinelPS.Data;

public static class LocaleNameResolver
{
    private static readonly object Gate = new();
    private static readonly Dictionary<string, Dictionary<string, string>> Tables = new(StringComparer.OrdinalIgnoreCase);
    private static bool loaded;

    public static string Resolve(string? localKey, string? language)
    {
        if (string.IsNullOrWhiteSpace(localKey)) return "";
        var key = localKey;
        var table = "Locale_System";
        var colon = localKey.IndexOf(':');
        if (colon > 0) { table = localKey[..colon]; key = localKey[(colon + 1)..]; }
        EnsureLoaded(table);
        var lang = language?.ToLowerInvariant() switch { "zh" or "zh-cn" => "zh-CN", "zhtw" or "zh-tw" => "zh-TW", "ko" => "ko", "ja" => "ja", "de" => "de", "th" => "th", "fr" => "fr", _ => "en" };
        if (!Tables.TryGetValue(table + "|" + lang, out var rows) || !rows.TryGetValue(key, out var value))
            return localKey;
        return value;
    }

    private static void EnsureLoaded(string table)
    {
        lock (Gate)
        {
            if (loaded) return;
            var roots = new[] { Path.Combine(AppContext.BaseDirectory, "cache", "local-locale") }.Where(Directory.Exists);
            foreach (var root in roots)
            foreach (var file in Directory.EnumerateFiles(root, "Locale_*.lsc", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var bytes = File.ReadAllBytes(file);
                    var sqlite = NkdbDecryptor.Decrypt(bytes);
                    var temp = Path.Combine(Path.GetTempPath(), $"epinelps-{Guid.NewGuid():N}.db");
                    try
                    {
                        File.WriteAllBytes(temp, sqlite);
                        using (var disk = new SqliteConnection($"Data Source={temp};Pooling=False"))
                        {
                            disk.Open();
                            using var tableCommand = disk.CreateCommand();
                            tableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' LIMIT 1";
                            var name = tableCommand.ExecuteScalar() as string;
                            if (string.IsNullOrEmpty(name)) continue;
                            foreach (var lang in new[] { "zh-CN", "zh-TW", "en", "ja", "ko", "de", "th", "fr" })
                            {
                                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                using var q = disk.CreateCommand();
                                q.CommandText = $"SELECT Key, \"{lang}\" FROM [{name}]";
                                using var reader = q.ExecuteReader();
                                while (reader.Read()) if (!reader.IsDBNull(1)) map[reader.GetString(0)] = reader.GetString(1);
                                Tables[name + "|" + lang] = map;
                            }
                        }
                    }
                    finally { if (File.Exists(temp)) File.Delete(temp); }
                }
                catch (Exception ex) { Console.Error.WriteLine($"Failed to load locale file {file}: {ex.Message}"); }
            }
            loaded = true;
        }
    }
}
