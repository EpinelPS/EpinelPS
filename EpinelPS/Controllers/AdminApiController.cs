using EpinelPS.Controllers.AdminPanel;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models.Admin;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;
using Paseto;
using Paseto.Builder;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Controllers;

[Route("adminapi")]
[ApiController]
public class AdminApiController(GameContext DbContext) : ControllerBase
{
    private static readonly AsyncLocal<string?> RequestLanguage = new();
    private readonly GameContext dbContext = DbContext;
    private static readonly MD5 md5 = MD5.Create();

    [HttpPost]
    [Route("login")]
    public LoginApiResponse Login([FromBody] LoginApiBody b)
    {
        SdkUser? user = null;
        bool nullusernames = false;
        if (b.Username != null && b.Password != null)
        {
            string passwordHash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(b.Password))).ToLower();
            foreach (var item in dbContext.SdkUsers)
            {
                if (item.Email == b.Username && item.PasswordHash != null)
                {
                    if (item.PasswordHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase))
                    {
                        user = item;
                    }
                }
            }
        }
        else
        {
            nullusernames = true;
        }

        if (user == null)
        {
            return nullusernames
                ? new LoginApiResponse() { Message = "Please enter a username and password" }
                : new LoginApiResponse() { Message = "Username or password is incorrect" };
        }
        else
        {
            if (user.IsAdmin)
            {
                string tok = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                    .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                    .AddClaim("userId", user.ID)
                    .IssuedAt(DateTime.UtcNow)
                    .Expiration(DateTime.UtcNow.AddDays(2))
                    .Encode();
                HttpContext.Response.Cookies.Append("token", tok);
                return new LoginApiResponse() { OK = true, Token = tok };
            }
            else
            {
                return new LoginApiResponse() { Message = "User is not an administrator." };
            }
        }
    }

    [HttpPost("RegisterAccount")]
    public RunCmdResponse RegisterAccount([FromBody] RegisterAccountReg req)
    {
        if (!AdminController.CheckAuth(HttpContext) && JsonDb.Instance.Users.Count != 0) return new RunCmdResponse() { error = "bad token" };

        if (dbContext.SdkUsers.Where(x => x.Email == req.Email).Count() != 0)
        {
            return new RunCmdResponse() { error = $"Email {req.Email} already exists" };
        }

        ulong uid = (ulong)new Random().Next(1, int.MaxValue);

        // Check if we havent generated a UID that exists
        foreach (User item in JsonDb.Instance.Users)
        {
            if (item.ID == uid)
            {
                uid -= (ulong)new Random().Next(1, 1221);
            }
        }

        bool admin = JsonDb.Instance.Users.Count == 0;

        JsonDb.Instance.Users.Add(new User()
        {
            ID = uid
        });

        dbContext.SdkUsers.Add(new SdkUser()
        {
            ID = uid,
            Email = req.Email,
            PasswordHash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(req.Password))).ToLower(),
            RegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsAdmin = admin,
            PlayerName = "Player_" + Rng.RandomString(8),
        });

        dbContext.Users.Add(new GameUser()
        {
            ID = uid // todo remove later
        });

        JsonDb.Save();
        dbContext.SaveChanges();

        return new RunCmdResponse() { ok = true };
    }

    [HttpPost("RunCmd")]
    public async Task<RunCmdResponse> RunCmd([FromBody] RunCmdRequest req)
    {
        if (!AdminController.CheckAuth(HttpContext)) return new RunCmdResponse() { error = "bad token" };

        switch (req.cmdName)
        {
            case "reloadDb":
                JsonDb.Reload();
                return RunCmdResponse.OK;
            case "completestage":
                return AdminCommands.CompleteStage(ulong.Parse(req.p1), req.p2);
            case "completeallstages":
                return AdminCommands.CompleteAllStages(ulong.Parse(req.p1));
            case "addallcharacters":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.AddAllCharacters(user);
                }
            case "addallcostumes":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.AddAllCostumes(user);
                }
            case "addallmaterials":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.AddAllMaterials(user, int.Parse(req.p2));
                }
            case "SetLevel":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.SetCharacterLevel(user, int.Parse(req.p2));
                }
            case "SetSkillLevel":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.SetSkillLevel(user, int.Parse(req.p2));
                }
            case "SetCoreLevel":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.SetCoreLevel(user, int.Parse(req.p2));
                }
            case "finishalltutorials":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.FinishAllTutorials(user);
                }
            case "AddCharacter":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.AddCharacter(user, int.Parse(req.p2));
                }
            case "AddItem":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };

                    string[] s = req.p2.Split("-");
                    return AdminCommands.AddItem(user, int.Parse(s[0]), int.Parse(s[1]));
                }
            case "SendMail":
                {
                    string[] parts = req.p1.Split('|');
                    if (parts.Length < 6)
                        return new RunCmdResponse() { error = "Insufficient parameters" };
                    if (!ulong.TryParse(parts[0], out ulong userId))
                        return new RunCmdResponse() { error = "Invalid user ID" };
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
                    if (user == null)
                        return new RunCmdResponse() { error = "User not found" };
                    if (!int.TryParse(parts[1], out int senderId))
                        return new RunCmdResponse() { error = "Invalid sender ID" };
                    string title = parts[2];
                    string content = parts[3];
                    if (!int.TryParse(parts[4], out int validDays))
                        return new RunCmdResponse() { error = "Invalid validity days" };
                    var attachments = new List<MailAttachment>();
                    string attachmentsParam = parts.Length > 5 ? parts[5] : "";

                    if (!string.IsNullOrEmpty(attachmentsParam))
                    {
                        foreach (var item in attachmentsParam.Split(','))
                        {
                            string[] attParts = item.Split('-');
                            if (attParts.Length != 3) continue;

                            if (int.TryParse(attParts[0], out int type) &&
                                int.TryParse(attParts[1], out int id) &&
                                int.TryParse(attParts[2], out int count))
                            {
                                attachments.Add(new MailAttachment
                                {
                                    Type = type,
                                    Id = id,
                                    Count = count
                                });
                            }
                        }
                    }

                    return AdminCommands.SendMail(user, senderId, title, content, validDays, attachments);
                }
            case "updateServer":
                {
                    return await AdminCommands.UpdateResources();
                }
        }
        return new RunCmdResponse() { error = "Not implemented" };
    }

    private static string LookupRealName(string nameLocalkey)
    {
        var localized = LocaleNameResolver.Resolve(nameLocalkey, RequestLanguage.Value);
        if (!string.Equals(localized, nameLocalkey, StringComparison.Ordinal)) return localized;
        return RealNameOrCleaned(nameLocalkey ?? "");
    }

    [HttpGet]
    [Route("searchGameData")]
    public IActionResult SearchGameData([FromQuery] int type, [FromQuery] string? q, [FromQuery] string? lang)
    {
        RequestLanguage.Value = lang;
        var results = new List<object>();
        var query = q?.Trim().ToLowerInvariant() ?? "";

        switch (type)
        {
            case 4: // Character
                SearchCharacters(query, results);
                break;
            case 5: // Item
                SearchDict(GameData.Instance.itemMaterialTable, r => r.NameLocalkey, query, results);
                SearchDict(GameData.Instance.ItemEquipTable, r => r.NameLocalkey, query, results);
                SearchDict(GameData.Instance.ConsumableItems, r => r.NameLocalkey, query, results);
                SearchDict(GameData.Instance.PieceItems, r => r.NameLocalkey, query, results);
                SearchDict(GameData.Instance.ItemHarmonyCubeTable, r => r.NameLocalkey, query, results);
                break;
            case 6: // Frame
                SearchDict(GameData.Instance.userFrameTable, r => r.NameLocalkey, query, results);
                break;
            case 8: // BGM / Jukebox theme
                foreach (var kv in GameData.Instance.jukeboxListDataRecords)
                {
                    var name = kv.Value.Name ?? kv.Value.Bgm ?? kv.Key.ToString();
                    if (string.IsNullOrEmpty(query) || name.Contains(query, StringComparison.OrdinalIgnoreCase) || kv.Key.ToString().Contains(query))
                        results.Add(new { id = kv.Key, name });
                }
                SearchDict(GameData.Instance.jukeboxThemeDataRecords, r => r.NameLocalkey, query, results);
                break;
            case 10: // LiveWallpaper
                SearchDict(GameData.Instance.LiveWallpaperTable, r => r.NameLocalkey, query, results);
                break;
            case 12: // Costume
                foreach (var kv in GameData.Instance.CharacterCostumeTable)
                {
                    var raw = kv.Value.CostumeNameLocale ?? "";
                    var cleaned = LookupRealName(raw);
                    var character = GameData.Instance.CharacterTable.Values.FirstOrDefault(c => c.ResourceId == kv.Value.ResourceId);
                    var characterName = character == null ? "" : LookupRealName(character.NameLocalkey ?? "");
                    var displayName = string.IsNullOrWhiteSpace(characterName) || string.IsNullOrWhiteSpace(cleaned)
                        ? (string.IsNullOrWhiteSpace(cleaned) ? kv.Key.ToString() : cleaned)
                        : $"{characterName} - {cleaned}";
                    if (string.IsNullOrEmpty(query) ||
                        raw.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        cleaned.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        characterName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        displayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.ToString().Contains(query))
                    {
                        results.Add(new { id = kv.Key, name = displayName });
                    }
                }
                break;
            case 43: // FavoriteItem
                SearchDict(GameData.Instance.FavoriteItemTable, r => r.NameLocalkey, query, results);
                break;
            case 44: // ProfileCardObject
                SearchDict(GameData.Instance.ProfileCardObjectTable, r => r.NameLocalkey, query, results);
                break;
            case 46: // UserTitle
                SearchDict(GameData.Instance.userTitleRecords, r => r.NameLocaleKey, query, results);
                break;
            case 48: // Album
                SearchDict(GameData.Instance.albumResourceRecords, r => r.ScenarioNameLocalkey, query, results);
                break;
        }

        return Ok(results.Take(5000));
    }

    private static void SearchDict<T>(Dictionary<int, T> dict, Func<T, string?> nameSelector, string query, List<object> results) where T : class
    {
        if (string.IsNullOrEmpty(query))
        {
            foreach (var kv in dict)
                results.Add(new { id = kv.Key, name = LookupRealName(nameSelector(kv.Value) ?? kv.Key.ToString()) });
        }
        else
        {
            foreach (var kv in dict)
            {
                var raw = nameSelector(kv.Value) ?? "";
                var display = LookupRealName(raw);
                if (raw.Contains(query, StringComparison.OrdinalIgnoreCase) || display.Contains(query, StringComparison.OrdinalIgnoreCase) || kv.Key.ToString().Contains(query))
                    results.Add(new { id = kv.Key, name = display });
            }
        }
    }

    private static void SearchCharacters(string query, List<object> results)
    {
        var records = GameData.Instance.CharacterTable;
        var predecessor = records.Values.Where(r => r.GrowGrade > 0)
            .ToDictionary(r => r.GrowGrade, r => r.Id);
        foreach (var kv in records)
        {
            var stage = 1;
            var current = kv.Key;
            var visited = new HashSet<int>();
            while (predecessor.TryGetValue(current, out var previous) && visited.Add(current))
            {
                stage++;
                current = previous;
            }
            var display = $"{LookupRealName(kv.Value.NameLocalkey ?? kv.Key.ToString())}（stage {stage}）";
            var raw = kv.Value.NameLocalkey ?? "";
            if (string.IsNullOrEmpty(query) || raw.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                display.Contains(query, StringComparison.OrdinalIgnoreCase) || kv.Key.ToString().Contains(query))
                results.Add(new { id = kv.Key, name = display });
        }
    }

    private static string RealNameOrCleaned(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";

        // Handle "Locale_XXX:###_name" or "Locale XXX:###" formats
        if (raw.StartsWith("Locale_", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("Locale ", StringComparison.OrdinalIgnoreCase))
        {
            var result = raw.StartsWith("Locale_") ? raw.Substring(7) : raw.Substring(7);
            foreach (var suf in new[] { "_Name", "_Desc", "_Locale", "_NameLocale", "_name", "_desc", "_locale" })
            {
                if (result.EndsWith(suf, StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Substring(0, result.Length - suf.Length);
                    break;
                }
            }
            return result;
        }

        // Clean up raw localization key
        var s = raw;
        if (s.StartsWith("Local_", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(6);
        var prefixes = new[] { "Character_", "Item_", "Equip_", "Costume_", "Material_",
            "Consume_", "Piece_", "HarmonyCube_", "Frame_", "Wallpaper_", "Title_",
            "FavoriteItem_", "ProfileCard_", "Album_", "Bgm_", "Jukebox_" };
        foreach (var prefix in prefixes)
        {
            if (s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(prefix.Length);
                break;
            }
        }
        var suffixes = new[] { "_Name", "_Desc", "_Description", "_Locale", "_Localkey", "_Localekey", "_Text" };
        foreach (var suf in suffixes)
        {
            if (s.EndsWith(suf, StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(0, s.Length - suf.Length);
                break;
            }
        }
        s = s.Replace("_", " ");
        return s.Trim();
    }

}
