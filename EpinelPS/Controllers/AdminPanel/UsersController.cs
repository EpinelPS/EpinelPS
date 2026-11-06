using EpinelPS.Database;
using EpinelPS.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using EpinelPS.Data;
using EpinelPS.Services;

namespace EpinelPS.Controllers.AdminPanel;

[Route("admin/Users")]
public class UsersController(ILogger<UsersController> logger, GameContext dbContext, MessengerAdminService messengerAdminService) : Controller
{
    private readonly ILogger<UsersController> _logger = logger;
    private readonly GameContext _db = dbContext;
    private readonly MessengerAdminService _messengerAdmin = messengerAdminService;
    private static readonly MD5 sha = MD5.Create();
    private readonly Dictionary<string, Dictionary<int, double>> _overloadOptions = new Dictionary<string, Dictionary<int, double>>
    {
        ["EleDmg"] = new Dictionary<int, double>
        {
            { 7000501, 9.54 }, { 7000502, 10.94 }, { 7000503, 12.34 },
            { 7000504, 13.75 }, { 7000505, 15.15 }, { 7000506, 16.55 },
            { 7000507, 17.95 }, { 7000508, 19.35 }, { 7000509, 20.75 },
            { 7000510, 22.15 }, { 7000511, 23.56 }, { 7000512, 24.96 },
            { 7000513, 26.36 }, { 7000514, 27.76 }, { 7000515, 29.16 }
        },
        ["Hit"] = new Dictionary<int, double>
        {
            { 7000601, 4.77 }, { 7000602, 5.47 }, { 7000603, 6.18 },
            { 7000604, 6.88 }, { 7000605, 7.59 }, { 7000606, 8.29 },
            { 7000607, 9.00 }, { 7000608, 9.70 }, { 7000609, 10.40 },
            { 7000610, 11.11 }, { 7000611, 11.81 }, { 7000612, 12.52 },
            { 7000613, 13.22 }, { 7000614, 13.93 }, { 7000615, 14.63 }
        },
        ["Ammo"] = new Dictionary<int, double>
        {
            { 7000701, 27.84 }, { 7000702, 31.95 }, { 7000703, 36.06 },
            { 7000704, 40.17 }, { 7000705, 44.28 }, { 7000706, 48.39 },
            { 7000707, 52.50 }, { 7000708, 56.60 }, { 7000709, 60.71 },
            { 7000710, 64.82 }, { 7000711, 68.93 }, { 7000712, 73.04 },
            { 7000713, 77.15 }, { 7000714, 81.26 }, { 7000715, 85.37 }
        },
        ["Atk"] = new Dictionary<int, double>
        {
            { 7000801, 4.77 }, { 7000802, 5.47 }, { 7000803, 6.18 },
            { 7000804, 6.88 }, { 7000805, 7.59 }, { 7000806, 8.29 },
            { 7000807, 9.00 }, { 7000808, 9.70 }, { 7000809, 10.40 },
            { 7000810, 11.11 }, { 7000811, 11.81 }, { 7000812, 12.52 },
            { 7000813, 13.22 }, { 7000814, 13.93 }, { 7000815, 14.63 }
        },
        ["ChgDmg"] = new Dictionary<int, double>
        {
            { 7000901, 4.77 }, { 7000902, 5.47 }, { 7000903, 6.18 },
            { 7000904, 6.88 }, { 7000905, 7.59 }, { 7000906, 8.29 },
            { 7000907, 9.00 }, { 7000908, 9.70 }, { 7000909, 10.40 },
            { 7000910, 11.11 }, { 7000911, 11.81 }, { 7000912, 12.52 },
            { 7000913, 13.22 }, { 7000914, 13.93 }, { 7000915, 14.63 }
        },
        ["ChgSpd"] = new Dictionary<int, double>
        {
            { 7001001, 1.98 }, { 7001002, 2.28 }, { 7001003, 2.57 },
            { 7001004, 2.86 }, { 7001005, 3.16 }, { 7001006, 3.45 },
            { 7001007, 3.75 }, { 7001008, 4.04 }, { 7001009, 4.33 },
            { 7001010, 4.63 }, { 7001011, 4.92 }, { 7001012, 5.21 },
            { 7001013, 5.51 }, { 7001014, 5.80 }, { 7001015, 6.09 }
        },
        ["CritDmg"] = new Dictionary<int, double>
        {
            { 7001201, 6.64 }, { 7001202, 7.62 }, { 7001203, 8.60 },
            { 7001204, 9.58 }, { 7001205, 10.56 }, { 7001206, 11.54 },
            { 7001207, 12.52 }, { 7001208, 13.50 }, { 7001209, 14.48 },
            { 7001210, 15.46 }, { 7001211, 16.44 }, { 7001212, 17.42 },
            { 7001213, 18.40 }, { 7001214, 19.38 }, { 7001215, 20.36 }
        },
        ["Crit"] = new Dictionary<int, double>
        {
            { 7001101, 2.30 }, { 7001102, 2.64 }, { 7001103, 2.98 },
            { 7001104, 3.32 }, { 7001105, 3.66 }, { 7001106, 4.00 },
            { 7001107, 4.35 }, { 7001108, 4.69 }, { 7001109, 5.03 },
            { 7001110, 5.37 }, { 7001111, 5.71 }, { 7001112, 6.05 },
            { 7001113, 6.39 }, { 7001114, 6.73 }, { 7001115, 7.07 }
        },
        ["Def"] = new Dictionary<int, double>
        {
            { 7001301, 4.77 }, { 7001302, 5.47 }, { 7001303, 6.18 },
            { 7001304, 6.88 }, { 7001305, 7.59 }, { 7001306, 8.29 },
            { 7001307, 9.00 }, { 7001308, 9.70 }, { 7001309, 10.40 },
            { 7001310, 11.11 }, { 7001311, 11.81 }, { 7001312, 12.52 },
            { 7001313, 13.22 }, { 7001314, 13.93 }, { 7001315, 14.63 }
        }
    };

    public IActionResult Index()
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        return View(_db.SdkUsers);
    }

    [Route("Delete/{id}")]
    public IActionResult Delete(ulong id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        SdkUser? sdkUser = _db.SdkUsers.Find(id);
        GameUser? gameUser = _db.Users.Find(id);
        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == id);
        if (sdkUser == null || gameUser == null || user == null)
            return NotFound();

        return View(new DeleteUserModel
        {
            ID = id,
            Email = sdkUser.Email,
            Password = sdkUser.PasswordHash,
            IsAdmin = sdkUser.IsAdmin,
            PlayerName = sdkUser.PlayerName,
            Nickname = gameUser.Nickname,
            IsBanned = user.IsBanned
        });
    }

    [Route("Delete/{id}"), ActionName("Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(ulong id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        SdkUser? sdkUser = _db.SdkUsers.Find(id);
        GameUser? gameUser = _db.Users.Find(id);
        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == id);
        if (sdkUser == null || gameUser == null || user == null)
            return NotFound();

        // Remove the trigger rows explicitly before deleting the GameUser row.
        // This works regardless of the database provider's cascade configuration.
        List<TriggerModelNew> triggerRows = _db.Triggers.Where(trigger => trigger.UserId == id).ToList();
        _db.Triggers.RemoveRange(triggerRows);

        // Same for the character toggle rows
        List<CharacterToggleModel> toggleRows = _db.CharacterToggles.Where(toggle => toggle.UserId == id).ToList();
        _db.CharacterToggles.RemoveRange(toggleRows);

        _db.Users.Remove(gameUser);
        _db.SdkUsers.Remove(sdkUser);
        _db.SaveChanges();

        JsonDb.Instance.Users.Remove(user);
        JsonDb.Save();

        return RedirectToAction(nameof(Index));
    }

    [Route("Modify/{id}")]
    public IActionResult Modify(ulong id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == id);
        if (user == null)
        {
            return NotFound();
        }
        var sdkUser = _db.SdkUsers.Find(id);
        if (sdkUser == null) return NotFound();

        var gameUser = _db.Users.Find(id);
        if (gameUser == null) return NotFound();

        return View(
            new ModUserModel()
            {
                IsAdmin = sdkUser.IsAdmin,
                IsBanned = user.IsBanned,
                Nickname = gameUser.Nickname ?? "Unknown nickname",
                sickpulls = user.sickpulls,
                Username = sdkUser.Email ?? "Unknown username",
                ID = user.ID,
                Characters = BuildCharacterList(user),
                OverloadOptions = _overloadOptions
            }
        );
    }

    /// <summary>
    /// Builds the character list of the user. The gear detail is loaded on demand
    /// by CharacterDetail so that the page stays small.
    /// </summary>
    private static List<ModUserCharacterModel> BuildCharacterList(User user)
    {
        Dictionary<long, EquipmentAwakeningData> gearDictionary = BuildGearDictionary(user);
        Dictionary<long, List<DbItemData>> itemsByCsn = user.Items
            .GroupBy(i => i.Csn)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<ModUserCharacterModel> characters = [];

        foreach (CharacterModel character in user.Characters)
        {
            CharacterRecord? charData = GameData.Instance.CharacterTable.TryGetValue(character.Tid, out var record) ? record : null;

            int gearCount = itemsByCsn.TryGetValue(character.Csn, out List<DbItemData>? items)
                ? items.Count(i => gearDictionary.ContainsKey(i.Isn))
                : 0;

            characters.Add(new ModUserCharacterModel()
            {
                Csn = character.Csn,
                Tid = character.Tid,
                Name = AdminApiController.LookupRealName(charData?.NameLocalkey ?? ""),
                Manufacturer = charData?.CorporationSubType == CorporationSubType.OVERSPEC
                    ? charData.CorporationSubType.ToString()
                    : charData?.Corporation.ToString() ?? "Unknown",
                Level = user.GetCharacterLevel(character.Csn, character.Level),
                IsSynchro = user.GetSynchro(character.Csn),
                Skill1 = character.Skill1Lvl,
                Skill2 = character.Skill2Lvl,
                BurstSkill = character.UltimateLevel,
                Grade = character.Grade,
                BreakCount = GetBreakCount(character.Grade),
                Rarity = charData?.OriginalRare.ToString() ?? "Unknown",
                ElementName = GetElementName(charData),
                WeaponType = GetWeaponType(charData),
                GearCount = gearCount
            });
        }

        return characters
            .OrderBy(c => c.Manufacturer)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Converts the stored GradeCoreId into the number of limit breaks.
    /// SR uses 101..103, SSR uses 1..11 and R cannot be limit broken.
    /// </summary>
    private static int GetBreakCount(int grade)
    {
        if (grade >= 101 && grade <= 103) return grade - 100;
        if (grade >= 1 && grade <= 11) return grade;
        return 0;
    }

    /// <summary>
    /// Returns the localized element name (code) of a character.
    /// </summary>
    private static string GetElementName(CharacterRecord? charData)
    {
        if (charData?.ElementId is not { Length: > 0 }) return "";
        if (!GameData.Instance.ElementTable.TryGetValue(charData.ElementId[0], out ElementRecord? element)) return "";
        return AdminApiController.LookupRealName(element.ElementNameLocalekey ?? "");
    }

    /// <summary>
    /// Returns the weapon type of a character.
    /// </summary>
    private static string GetWeaponType(CharacterRecord? charData)
    {
        if (charData == null) return "";
        if (!GameData.Instance.CharacterShotTable.TryGetValue(charData.ShotId, out CharacterShotRecord? shot)) return "";
        return shot.WeaponType == WeaponType.None ? "" : shot.WeaponType.ToString();
    }

    /// <summary>
    /// Returns the gear detail of a single character for the modify page modal.
    /// </summary>
    [Route("CharacterDetail/{id}")]
    public IActionResult CharacterDetail(ulong id, int csn)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == id);
        if (user == null) return NotFound();

        CharacterModel? character = user.Characters.FirstOrDefault(c => c.Csn == csn);
        if (character == null) return NotFound();

        Dictionary<long, EquipmentAwakeningData> gearDictionary = BuildGearDictionary(user);
        List<DbItemData> items = user.Items.Where(i => i.Csn == csn).ToList();
        List<DbItemData> gearItems = items.Where(i => gearDictionary.ContainsKey(i.Isn)).ToList();

        CharacterRecord? charData = GameData.Instance.CharacterTable.TryGetValue(character.Tid, out var record) ? record : null;

        ModUserCharacterDetailModel model = new()
        {
            UserId = id,
            OverloadOptions = _overloadOptions,
            Character = new ModUserCharacterModel()
            {
                Csn = character.Csn,
                Tid = character.Tid,
                Name = AdminApiController.LookupRealName(charData?.NameLocalkey ?? ""),
                Manufacturer = charData?.CorporationSubType == CorporationSubType.OVERSPEC
                    ? charData.CorporationSubType.ToString()
                    : charData?.Corporation.ToString() ?? "Unknown",
                Level = user.GetCharacterLevel(character.Csn, character.Level),
                IsSynchro = user.GetSynchro(character.Csn),
                Skill1 = character.Skill1Lvl,
                Skill2 = character.Skill2Lvl,
                BurstSkill = character.UltimateLevel,
                Grade = character.Grade,
                BreakCount = GetBreakCount(character.Grade),
                Rarity = charData?.OriginalRare.ToString() ?? "Unknown",
                ElementName = GetElementName(charData),
                WeaponType = GetWeaponType(charData),
                GearCount = gearItems.Count
            },
            Items = items,
            Gears =
            [
                .. gearItems.Select(gear => new GearModel()
                {
                    GearIsn = gear.Isn,
                    GearName = GetGearName(gear.ItemType),
                    GearType = GetGearType(gear.ItemType),
                    GearLevel = gear.Level,
                    Option1Id = gearDictionary[gear.Isn].Option.Option1Id,
                    Option1Lock = gearDictionary[gear.Isn].Option.Option1Lock,
                    Option2Id = gearDictionary[gear.Isn].Option.Option2Id,
                    Option2Lock = gearDictionary[gear.Isn].Option.Option2Lock,
                    Option3Id = gearDictionary[gear.Isn].Option.Option3Id,
                    Option3Lock = gearDictionary[gear.Isn].Option.Option3Lock
                })
            ]
        };

        return PartialView("_CharacterDetail", model);
    }

    /// <summary>
    /// Awakening rows can exist multiple times for the same item, the newest one wins.
    /// </summary>
    private static Dictionary<long, EquipmentAwakeningData> BuildGearDictionary(User user)
    {
        return user.EquipmentAwakenings
            .GroupBy(a => a.Isn)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.IsNewData).First());
    }

    /// <summary>
    /// Returns the gear slot of an item, for example Head or Body.
    /// </summary>
    private static string GetGearType(int itemType)
    {
        return GameData.Instance.GetItemSubType(itemType) switch
        {
            ItemSubType.ModuleA => "Head",
            ItemSubType.ModuleB => "Body",
            ItemSubType.ModuleC => "Arm",
            ItemSubType.ModuleD => "Leg",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Returns the localized name of an item.
    /// </summary>
    private static string GetGearName(int itemType)
    {
        if (GameData.Instance.ItemEquipTable.TryGetValue(itemType, out var equip))
        {
            return AdminApiController.LookupRealName(equip.NameLocalkey ?? "");
        }

        return "";
    }

    [Route("ModifyGear/{id}"), ActionName("ModifyGear")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ModifyGear(
    ulong id,
    [FromForm] string submitType,
    [FromForm] Dictionary<long, GearUpdateModel> gearUpdates)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");
        var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == id);
        if (user == null) return NotFound();
        var sdkUser = _db.SdkUsers.Find(id);
        if (sdkUser == null) return NotFound();

        var gameUser = _db.Users.Find(id);
        if (gameUser == null) return NotFound();

        if (gearUpdates.Count == 0)return RedirectToAction("Modify", new { id = id });

        if (submitType == "saveAll")
        {
            foreach (var gearUpdate in gearUpdates.Values)
            {
                UpdateGear(user, gearUpdate.GearId, gearUpdate.Level,
                    gearUpdate.Option1, gearUpdate.Option2, gearUpdate.Option3);
            }
        }
        else if (submitType.StartsWith("saveCharacter_"))
        {
            foreach (var gearUpdate in gearUpdates.Values.Where(g => g.CharacterId == ulong.Parse(submitType.Replace("saveCharacter_", ""))))
            {
                UpdateGear(user, gearUpdate.GearId, gearUpdate.Level, gearUpdate.Option1, gearUpdate.Option2, gearUpdate.Option3);
            }
        }
        else if (submitType.StartsWith("updateGear_"))
        {
            var gearId = long.Parse(submitType.Replace("updateGear_", ""));
            if (gearUpdates.TryGetValue(gearId, out var gearUpdate))
            {
                UpdateGear(user, gearUpdate.GearId, gearUpdate.Level,
                    gearUpdate.Option1, gearUpdate.Option2, gearUpdate.Option3);
            }
        }

        // Persist the gear changes, they are only kept in memory otherwise
        JsonDb.Save();

        return RedirectToAction("Modify", new { id = id });
    }

    private static void UpdateGear(User user, long gearId, int level, int option1, int option2, int option3)
    {
        var gearAwakening = user.EquipmentAwakenings.FirstOrDefault(x => x.Isn == gearId);
        var item = user.Items.FirstOrDefault(x => x.Isn == gearId);

        if (gearAwakening == null || item == null) return;
        item.Level = level;
        gearAwakening.Option.Option1Id = option1;
        gearAwakening.Option.Option2Id = option2;
        gearAwakening.Option.Option3Id = option3;

        System.Console.WriteLine($"Updated gear {gearId}: Level={level}, Options={option1},{option2},{option3}");
    }

    [Route("Modify/{id}"), ActionName("Modify")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DoModifyUser(ulong id, [FromForm] ModUserModel toSet)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        if (!ModelState.IsValid) throw new Exception("model state invalid");

        User? user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
        if (user == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(toSet.Username))
            throw new Exception("username cannot be empty");

        var sdkUser = _db.SdkUsers.Find(id);
        if (sdkUser == null) return NotFound();
        var gameUser = _db.Users.Find(id);
        if (gameUser == null) return NotFound();
        sdkUser.Email = toSet.Username;
        sdkUser.IsAdmin = toSet.IsAdmin;
        gameUser.Nickname = toSet.Nickname;
        gameUser.Description = toSet.Description;
        _db.SaveChanges();

        user.sickpulls = toSet.sickpulls;
        user.IsBanned = toSet.IsBanned;
        JsonDb.Save();

        return View(new ModUserModel()
        {
            IsAdmin = sdkUser.IsAdmin,
            IsBanned = user.IsBanned,
            Nickname = gameUser.Nickname,
            sickpulls = user.sickpulls,
            Username = sdkUser.Email,
            ID = user.ID
        });
    }

    [Route("Currency/{id}")]
    public IActionResult Currency(ulong id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        User? user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
        if (user == null)
        {
            return NotFound();
        }

        return View(
            new ModUserCurrencyModel()
            {
                ID = user.ID,
                Current = user.Currency
            }
        );
    }

    [Route("Currency/{id}"), ActionName("Currency")]
    [HttpPost]
    public IActionResult CurrencyModify(ulong id, [FromForm] ModUserCurrencyModel model)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        User? user = JsonDb.Instance.Users.Where(x => x.ID == id).FirstOrDefault();
        if (user == null)
        {
            return NotFound();
        }

        user.AddCurrency(model.ToModify, model.Amount);
        JsonDb.Save();

        return View(
            new ModUserCurrencyModel()
            {
                ID = user.ID,
                Current = user.Currency
            }
        );
    }

    [Route("Messenger/{id}")]
    public IActionResult Messenger(ulong id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        User? user = JsonDb.Instance.Users.FirstOrDefault(user => user.ID == id);
        if (user == null) return NotFound();

        return View(_messengerAdmin.BuildModel(user));
    }

    [Route("Messenger/{id}/Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateMessengerMessage(ulong id, [FromForm] CreateMessengerMessageModel model)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");
        User? user = JsonDb.Instance.Users.FirstOrDefault(user => user.ID == id);
        if (user == null) return NotFound();

        if (_messengerAdmin.TryCreate(user, model, out string error))
        {
            JsonDb.Save();
            TempData["MessengerSuccess"] = "Message created. No trigger was added.";
        }
        else
        {
            TempData["MessengerError"] = error;
        }

        return RedirectToAction(nameof(Messenger), new { id });
    }

    [Route("Messenger/{id}/Update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateMessengerMessage(ulong id, [FromForm] UpdateMessengerMessageModel model)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");
        User? user = JsonDb.Instance.Users.FirstOrDefault(user => user.ID == id);
        if (user == null) return NotFound();

        if (_messengerAdmin.TryUpdateState(user, model, out string error))
        {
            JsonDb.Save();
            TempData["MessengerSuccess"] = "Message state updated. No trigger was changed.";
        }
        else
        {
            TempData["MessengerError"] = error;
        }

        return RedirectToAction(nameof(Messenger), new { id });
    }

    [Route("Messenger/{id}/Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteMessengerMessage(ulong id, [FromForm] long seq)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");
        User? user = JsonDb.Instance.Users.FirstOrDefault(user => user.ID == id);
        if (user == null) return NotFound();

        if (_messengerAdmin.TryDelete(user, seq, out string error))
        {
            JsonDb.Save();
            TempData["MessengerSuccess"] = "Erroneous opener removed. A restore snapshot was saved.";
        }
        else
        {
            TempData["MessengerError"] = error;
        }

        return RedirectToAction(nameof(Messenger), new { id });
    }

    [Route("Messenger/{id}/Restore")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RestoreMessengerMessage(ulong id, [FromForm] string auditId)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");
        User? user = JsonDb.Instance.Users.FirstOrDefault(user => user.ID == id);
        if (user == null) return NotFound();

        if (_messengerAdmin.TryRestore(user, auditId, out string error))
        {
            JsonDb.Save();
            TempData["MessengerSuccess"] = "Deleted message restored.";
        }
        else
        {
            TempData["MessengerError"] = error;
        }

        return RedirectToAction(nameof(Messenger), new { id });
    }

    [Route("SetPassword/{id}")]
    public IActionResult SetPassword(ulong id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        SdkUser? user = _db.SdkUsers.Find(id);
        if (user == null) return NotFound();

        return View(new ChangeUserPasswordModel()
        {
            Email = user.Email,
            ID = user.ID
        });
    }


    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Route("SetPassword")]
    [HttpPost, ActionName("SetPassword")]
    [ValidateAntiForgeryToken]
    public IActionResult SetPasswordConfirm(ulong? id)
    {
        if (!AdminController.CheckAuth(HttpContext)) return Redirect("/admin/");

        if (id == null)
        {
            return NotFound();
        }

        string? newPw = Request.Form["PasswordHash"];
        if (string.IsNullOrEmpty(newPw))
        {
            return BadRequest();
        }

        // TODO: use bcrypt
        SdkUser? user = _db.SdkUsers.Find(id);
        if (user == null) return NotFound();
        user.PasswordHash = Convert.ToHexString(sha.ComputeHash(Encoding.ASCII.GetBytes(newPw))).ToLower();
        _db.SaveChanges();

        return View(new ChangeUserPasswordModel()
        {
            Email = user.Email,
            ID = user.ID
        });
    }
    [Route("GetUsersList")]
    public IActionResult GetUsersList()
    {
        if (!AdminController.CheckAuth(HttpContext)) return Unauthorized();
        var users = _db.Users
            .Select(u => new { u.ID, u.Nickname })
            .ToList();
        return Ok(users);
    }
}
