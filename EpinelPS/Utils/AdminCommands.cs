using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Models.Admin;
using Google.Protobuf;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.Json;
using EpinelPS.LobbyServer.Tower;

namespace EpinelPS.Utils;

public class AdminCommands
{
    private static readonly HttpClient client;

    private static readonly string serverUrl = "global-lobby.nikke-kr.com";
    private static string connectingServer = serverUrl;
    private static string? serverIp;
    private static string? staticDataUrl;
    private static string? resourcesUrl;
    static AdminCommands()
    {
        // Use TLS 1.1 so that tencents cloudflare knockoff wont complain
        if (!OperatingSystem.IsLinux())
        {
            SocketsHttpHandler handler = new()
            {
                ConnectCallback = static async (context, cancellationToken) =>
                {
                    Socket socket = new(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                    try
                    {
                        await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);

                        SslStream sslStream = new(new NetworkStream(socket, ownsSocket: true));

                        // When using HTTP/2, you must also keep in mind to set options like ApplicationProtocols
                        await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                        {
                            TargetHost = connectingServer,
                            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls11

                        }, cancellationToken);

                        return sslStream;
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            };

            client = new(handler);
        }
        else
        {
            client = new();
        }
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream+protobuf");
    }
    public static RunCmdResponse CompleteAllStages(ulong userId)
    {
        // Find max chapter number
        var chapters = GameData.Instance.ChapterCampaignData.Values;
        int maxChapter = chapters.Max(c => c.Chapter);

        // Find max stage count for that chapter (main stages)
        // GetStageIdsForChapter uses a zero-based chapter index, while the
        // campaign table and admin command use the user-facing one-based
        // chapter number.
        int maxStage = GetNormalMainStages(maxChapter).Count;

        if (maxStage == 0) maxStage = 1;

        return CompleteStage(userId, $"{maxChapter}-{maxStage}");
    }

    public static RunCmdResponse CompleteStage(ulong userId, string input2)
    {
        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
        if (user == null) return new RunCmdResponse() { error = "invalid user ID" };

        try
        {
            var parts = input2.Split(['-', ' ', '_', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
                return new RunCmdResponse() { error = "Please provide chapter and stage numbers (e.g. '2 1' or '2-1')" };

            bool chapterParsed = int.TryParse(parts[0], out int chapterNumber);
            bool stageParsed = int.TryParse(parts[1], out int stageNumber);

            if (chapterParsed && stageParsed)
            {
                if (chapterNumber < 1 || stageNumber < 1)
                    return new RunCmdResponse() { error = "Chapter and stage number must be positive integers" };

                Console.WriteLine($"Chapter number: {chapterNumber}, Stage number: {stageNumber}");

                // Ensure starter characters exist
                ClearStage.EnsureDefaultCharacters(user);

                // Ensure prologue stages (chapter 0: 6000001..6000003) are completed
                var prologueStages = GameData.Instance.GetStageIdsForChapter(0, true)
                    .Select(stageId => GameData.Instance.GetStageData(stageId)!)
                    .OrderBy(stage => stage.Id);
                foreach (CampaignStageRecord stageData in prologueStages)
                {
                    if (!user.IsStageCompleted(stageData.Id))
                    {
                        ClearStage.CompleteStage(user, stageData.Id, true, recordMainQuest: false);
                    }
                }

                // Complete main stages up to target chapter and stage
                int lastClearedStageId = user.LastNormalStageCleared;
                for (int campaignChapter = 1; campaignChapter <= chapterNumber; campaignChapter++)
                {
                    List<CampaignStageRecord> stages = GetNormalMainStages(campaignChapter);
                    int stageLimit = campaignChapter == chapterNumber ? stageNumber : stages.Count;

                    if (stageLimit > stages.Count)
                    {
                        return new RunCmdResponse()
                        {
                            error = $"Chapter {campaignChapter} has only {stages.Count} normal main stages"
                        };
                    }

                    foreach (CampaignStageRecord stageData in stages.Take(stageLimit))
                    {
                        if (!user.IsStageCompleted(stageData.Id))
                        {
                            Console.WriteLine("Completing stage " + stageData.Id);
                            ClearStage.CompleteStage(user, stageData.Id, true, recordMainQuest: false);
                        }
                        lastClearedStageId = stageData.Id;
                    }
                }

                // Process scenario stages up to target chapter and stage
                for (int chapter = 1; chapter <= chapterNumber; chapter++)
                {
                    List<string> stages = [.. GameData.Instance.GetScenarioStageIdsForChapter(chapter)
                        .Where(stageId => GameData.Instance.IsValIdScenarioStage(stageId, chapterNumber, stageNumber))];

                    foreach (string? stage in stages)
                    {
                        if (!user.CompletedScenarios.Contains(stage))
                        {
                            user.CompletedScenarios.Add(stage);
                        }
                    }
                }

                // Ensure prologue map is initialized
                string prologueMapId = GameData.Instance.GetMapIdFromChapter(1, ChapterMod.Normal);
                if (!user.FieldInfoNew.ContainsKey(prologueMapId))
                {
                    user.FieldInfoNew.Add(prologueMapId, new FieldInfoNew());
                }

                // Ensure the field map for each chapter up to target is initialized
                // Note: In ChapterCampaignData, Prologue is chapter=0 (Id=1), Chapter 1 is chapter=1 (Id=2), Chapter 2 is chapter=2 (Id=3).
                // GetMapIdFromChapter(chapter) expects ChapterId (1-based: 1 for Prologue, 2 for Chapter 1, 3 for Chapter 2).
                for (int c = 1; c <= chapterNumber; c++)
                {
                    string mapId = GameData.Instance.GetMapIdFromChapter(c + 1, ChapterMod.Normal);
                    if (!user.FieldInfoNew.ContainsKey(mapId))
                    {
                        user.FieldInfoNew.Add(mapId, new FieldInfoNew());
                    }
                    // Invalidate stale local map json so the client fetches a fresh field state
                    user.MapJson.Remove(mapId);
                }

                user.LastNormalStageCleared = lastClearedStageId;

                // Sync main quest progression along the quest chain up to the last cleared stage
                List<int> validQuests = ClearStage.GetCompletedQuestsForStage(lastClearedStageId);
                HashSet<int> validQuestSet = [.. validQuests];

                // Prune any quests that were beyond this stage (cleans up any previous corruption)
                List<int> invalidQuests = user.MainQuestData.Keys.Where(k => !validQuestSet.Contains(k)).ToList();
                foreach (int badKey in invalidQuests)
                {
                    user.MainQuestData.Remove(badKey);
                }

                // Clean up any stale MainQuestClear triggers in database
                using (GameContext context = GameContext.CreateNew())
                {
                    var badTriggers = context.Triggers
                        .Where(t => t.UserId == user.ID && t.Type == Trigger.MainQuestClear && t.ConditionId <= 9999 && !validQuestSet.Contains(t.ConditionId))
                        .ToList();
                    if (badTriggers.Count > 0)
                    {
                        context.Triggers.RemoveRange(badTriggers);
                        context.SaveChanges();
                    }
                }

                HashSet<(Trigger Type, int ConditionId)> existingTriggers;
                using (GameContext context = GameContext.CreateNew())
                {
                    existingTriggers = context.Triggers
                        .Where(trigger => trigger.UserId == user.ID)
                        .AsEnumerable()
                        .Select(trigger => (trigger.Type, trigger.ConditionId))
                        .ToHashSet();
                }

                int completedQuestTriggers = 0;
                foreach (int questId in validQuests)
                {
                    user.MainQuestData.TryAdd(questId, false);

                    if (GameData.Instance.QuestDataRecords.TryGetValue(questId, out MainQuestRecord? quest))
                    {
                        if (quest.ConditionId != null && quest.ConditionId.Count > 0)
                        {
                            int campaignConditionId = quest.ConditionId[0].ConditionId;
                            if (campaignConditionId != 0 && existingTriggers.Add((Trigger.CampaignClear, campaignConditionId)))
                            {
                                user.AddTrigger(Trigger.CampaignClear, 1, campaignConditionId);
                            }
                        }
                    }

                    if (existingTriggers.Add((Trigger.MainQuestClear, questId)))
                    {
                        user.AddTrigger(Trigger.MainQuestClear, 1, questId);
                        completedQuestTriggers++;
                    }
                }

                // Reconcile ChapterClear triggers for all completed normal chapters up to target progression
                int completedChapterTriggers = 0;
                for (int c = 1; c <= chapterNumber; c++)
                {
                    List<CampaignStageRecord> chapterStages = GetNormalMainStages(c);
                    CampaignStageRecord? bossStage = chapterStages.LastOrDefault();
                    if (bossStage != null && user.IsStageCompleted(bossStage.Id))
                    {
                        if (existingTriggers.Add((Trigger.ChapterClear, bossStage.ChapterId)))
                        {
                            user.AddTrigger(Trigger.ChapterClear, 1, bossStage.ChapterId);
                            completedChapterTriggers++;
                        }
                    }
                }

                // Reconcile eligible Messenger openers
                MessengerMessageCreator.CreateAllEligibleOpeners(user);
                Logging.WriteLine($"[Admin] CompleteStage recorded {completedQuestTriggers} MainQuestClear and {completedChapterTriggers} ChapterClear triggers for user {user.ID} up to stage {lastClearedStageId}", LogType.Info);

                // Save changes to user data
                JsonDb.Save();

            }
            else
            {
                return new RunCmdResponse() { error = "Chapter and stage number must be valid integers" };
            }
        }
        catch (Exception ex)
        {
            return new RunCmdResponse() { error = "Exception: " + ex.ToString() };
        }

        return RunCmdResponse.OK;
    }

    public static RunCmdResponse CompleteAllEventStages(ulong userId)
    {
        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
        if (user == null) return new RunCmdResponse() { error = "invalId user ID" };

        int completedCount = 0;

        // Collect all triggers first, then batch-add to avoid DbContext threading issues
        var triggersToAdd = new List<(Trigger type, int value, int conditionId)>();

        // Build stageId -> eventId mapping via EventDungeonStageTable -> EventDungeonDifficultTable -> EventDungeonTable
        var stageToEvent = new Dictionary<int, int>();
        var eventStages = new Dictionary<int, List<int>>();

        foreach (var stageKv in GameData.Instance.EventDungeonStageTable)
        {
            int stageId = stageKv.Key;
            int stageGroup = stageKv.Value.Group;

            // Find the difficult record that matches this stage's group
            var difficult = GameData.Instance.EventDungeonDifficultTable.Values.FirstOrDefault(x => x.StageGroup == stageGroup);
            if (difficult == null) continue;

            // Find the event that matches this difficult's group
            var dungeon = GameData.Instance.EventDungeonTable.Values.FirstOrDefault(x => x.DifficultGroup == difficult.Group);
            if (dungeon == null) continue;

            int eventId = dungeon.Id;
            stageToEvent[stageId] = eventId;

            if (!eventStages.ContainsKey(eventId))
                eventStages[eventId] = new List<int>();
            eventStages[eventId].Add(stageId);

            triggersToAdd.Add((Trigger.EventStageClear, 1, stageId));
            completedCount++;
        }

        // Also trigger EventDungeonStageClear for all events and populate EventInfo
        foreach (var eventKv in GameData.Instance.EventDungeonTable)
        {
            int eventId = eventKv.Value.Id;
            triggersToAdd.Add((Trigger.EventDungeonStageClear, 1, eventId));

            // Populate EventInfo with all stage IDs cleared
            if (user.EventInfo.ContainsKey(eventId))
            {
                var eventData = user.EventInfo[eventId];
                if (eventStages.ContainsKey(eventId))
                {
                    foreach (var stageId in eventStages[eventId])
                    {
                        if (!eventData.ClearedStages.Contains(stageId))
                            eventData.ClearedStages.Add(stageId);
                    }
                    eventData.LastStage = eventStages[eventId].Max();
                }
            }
            else
            {
                var clearedStages = eventStages.ContainsKey(eventId) ? eventStages[eventId].ToList() : new List<int>();
                user.EventInfo.Add(eventId, new EventData()
                {
                    LastStage = clearedStages.Any() ? clearedStages.Max() : 0,
                    ClearedStages = clearedStages
                });
            }
        }

        // Batch-add triggers
        foreach (var (type, value, conditionId) in triggersToAdd)
        {
            user.AddTrigger(type, value, conditionId);
        }

        Console.WriteLine($"Completed {completedCount} event stages for user {userId}");
        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static List<CampaignStageRecord> GetNormalMainStages(int campaignChapter)
    {
        // GetStageIdsForChapter matches (data.ChapterId - 1 == campaignChapter).
        // For campaignChapter = 1, data.ChapterId = 2 (Chapter 1 stages 6001001..6001004).
        // For campaignChapter = 2, data.ChapterId = 3 (Chapter 2 stages 6002001..6002016).
        return [.. GameData.Instance.GetStageIdsForChapter(campaignChapter, true)
            .Select(stageId => GameData.Instance.GetStageData(stageId)
                ?? throw new Exception("failed to find stage " + stageId))
            .OrderBy(stage => stage.Id)];
    }


    /// <summary>
    /// Cheat: enroll every subquest and create its starting Messenger opener.
    /// This intentionally bypasses normal prerequisites and trigger checks,
    /// but preserves an already completed SubQuestData entry.
    /// </summary>
    public static RunCmdResponse UnlockAllSubquestMessages(ulong userId)
    {
        User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
        if (user == null) return new RunCmdResponse() { error = "invalId user ID" };

        int enrolledCount = 0;
        int createdCount = 0;
        foreach (SubQuestRecord subQuest in GameData.Instance.Subquests.Values)
        {
            if (!user.SubQuestData.ContainsKey(subQuest.Id))
            {
                user.SetSubQuest(subQuest.Id, false);
                enrolledCount++;
            }

            if (string.IsNullOrEmpty(subQuest.ConversationId))
                continue;

            MessengerDialogRecord? opener = GameData.Instance.Messages.Values.FirstOrDefault(message =>
                message.ConversationId == subQuest.ConversationId && message.IsOpener);
            if (opener == null)
                continue;

            if (user.MessengerData.Any(message => message.ConversationId == opener.ConversationId))
                continue;

            user.CreateMessage(opener);
            createdCount++;
        }

        Logging.WriteLine($"[Admin] UnlockAllSubquestMessages user={user.ID}, Enrolled={enrolledCount}, Created={createdCount}", LogType.Warning);
        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse AddAllCharacters(User user)
    {
        // Group characters by NameCode and always add those with GradeCoreId == 11, 103, and include GradeCoreId == 201
        List<CharacterRecord> allCharacters = [.. GameData.Instance.CharacterTable.Values
            .GroupBy(c => c.NameCode)  // Group by NameCode to treat same NameCode as one character                     3999 = marian
            .SelectMany(g => g.Where(c => c.GradeCoreId == 1 || c.GradeCoreId == 101 || c.GradeCoreId == 201 || c.NameCode == 3999))];

        foreach (CharacterRecord? character in allCharacters)
        {
            if (!user.HasCharacter(character.Id))
            {
                user.Characters.Add(new CharacterModel()
                {
                    CostumeId = 0,
                    Csn = user.GenerateUniqueCharacterId(),
                    Grade = 0,
                    Level = 1,
                    Skill1Lvl = 1,
                    Skill2Lvl = 1,
                    Tid = character.Id,  // Tid is the character ID
                    UltimateLevel = 1
                });

                user.BondInfo.Add(new() { NameCode = character.NameCode, Lv = 1 });
                user.AddTrigger(Trigger.ObtainCharacter, 1, character.NameCode);
                user.AddTrigger(Trigger.ObtainCharacterNew, 1, 0);
                if (character.OriginalRare == OriginalRareType.SSR)
                    user.AddTrigger(Trigger.ObtainCharacterSSR, 1);
            }
        }

        // A character can unlock a Messenger room even when the conversation
        // itself is gated by an already-completed campaign/event trigger.
        MessengerMessageCreator.CreateAllEligibleOpeners(user);

        JsonDb.Save();

        return RunCmdResponse.OK;
    }

    public static RunCmdResponse AddAllCostumes(User user)
    {
        foreach (var kv in GameData.Instance.CharacterCostumeTable)
        {
            user.AddUnique(user.CostumeList, kv.Key);
        }
        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse AddAllCollections(User user)
    {
        // 1. Set all characters' bond level to 30
        foreach (CharacterModel character in user.Characters)
        {
            if (!GameData.Instance.CharacterTable.TryGetValue(character.Tid, out var charRecord)) continue;

            NetUserAttractiveData? bondInfo = user.BondInfo.FirstOrDefault(b => b.NameCode == charRecord.NameCode);
            if (bondInfo == null)
            {
                bondInfo = new NetUserAttractiveData { NameCode = charRecord.NameCode };
                user.BondInfo.Add(bondInfo);
            }
            bondInfo.Lv = 30;
            bondInfo.Exp = 0;
        }

        // 2. Add all collections, equipping to matching character if found
        foreach (FavoriteItemRecord record in GameData.Instance.FavoriteItemTable.Values)
        {
            NetUserFavoriteItemData? item = user.FavoriteItems.FirstOrDefault(f => f.Tid == record.Id);
            if (item == null)
            {
                item = new NetUserFavoriteItemData
                {
                    FavoriteItemId = user.GenerateUniqueItemId(),
                    Tid = record.Id,
                    Csn = 0,
                    Lv = record.MaxLevel,
                    Exp = 0
                };
                user.FavoriteItems.Add(item);
            }

            // Try to equip to a character matching this collection's NameCode
            CharacterModel? match = user.Characters.FirstOrDefault(c =>
                GameData.Instance.CharacterTable.TryGetValue(c.Tid, out var cr) &&
                cr.NameCode == record.NameCode);

            if (match != null)
            {
                NetUserFavoriteItemData? other = user.FavoriteItems.FirstOrDefault(f => f.Csn == match.Csn && f.Tid != record.Id);
                if (other != null) other.Csn = 0;

                item.Csn = match.Csn;
                item.Lv = record.MaxLevel;
            }
        }

        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse SetAllBondLevel(User user, int level)
    {
        level = Math.Clamp(level, 1, 30);

        foreach (CharacterModel character in user.Characters)
        {
            if (!GameData.Instance.CharacterTable.TryGetValue(character.Tid, out var charRecord)) continue;

            NetUserAttractiveData? bondInfo = user.BondInfo.FirstOrDefault(b => b.NameCode == charRecord.NameCode);
            if (bondInfo == null)
            {
                bondInfo = new NetUserAttractiveData { NameCode = charRecord.NameCode };
                user.BondInfo.Add(bondInfo);
            }
            bondInfo.Lv = level;
            bondInfo.Exp = 0;
        }
        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse AddAllMaterials(User user, int amount)
    {
        foreach (ItemMaterialRecord tableItem in GameData.Instance.itemMaterialTable.Values)
        {
            DbItemData? item = user.Items.FirstOrDefault(i => i.ItemType == tableItem.Id);

            if (item == null)
            {
                user.Items.Add(new DbItemData
                {
                    Isn = user.GenerateUniqueItemId(),
                    ItemType = tableItem.Id,
                    Level = 1,
                    Exp = 1,
                    Count = amount
                });
            }
            else
            {
                item.Count += amount;
            }
        }

        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse AddAllEq(User user, int amount1, int amount2, int amount3)
    {
        if (amount1 > 0)
        {
            foreach (FavoriteItemRecord tableItem in GameData.Instance.FavoriteItemTable.Values)
            {
                foreach (int i in Enumerable.Range(0, amount1))
                {
                    user.FavoriteItems.Add(new NetUserFavoriteItemData
                    {
                        FavoriteItemId = user.GenerateUniqueItemId(),
                        Tid = tableItem.Id,
                        Csn = 0,
                        Lv = 15,
                        Exp = 0
                    });
                }
                JsonDb.Save();
            }
        }

        if (amount2 > 0)
        {
            foreach (ItemConsumeRecord tableItem in GameData.Instance.ConsumableItems.Values)
            {
                DbItemData? item = user.Items.FirstOrDefault(i => i.ItemType == tableItem.Id);

                if (item == null)
                {
                    user.Items.Add(new DbItemData
                    {
                        Isn = user.GenerateUniqueItemId(),
                        ItemType = tableItem.Id,
                        Level = 0,
                        Exp = 0,
                        Count = amount2
                    });
                }
                else
                {
                    item.Count += amount2;
                }
            }
            JsonDb.Save();
            foreach (ItemPieceRecord tableItem in GameData.Instance.PieceItems.Values)
            {
                DbItemData? item = user.Items.FirstOrDefault(i => i.ItemType == tableItem.Id);

                if (item == null)
                {
                    user.Items.Add(new DbItemData
                    {
                        Isn = user.GenerateUniqueItemId(),
                        ItemType = tableItem.Id,
                        Level = 0,
                        Exp = 0,
                        Count = amount2
                    });
                }
                else
                {
                    item.Count += amount2;
                }
            }
        }
        if (amount3 > 0)
        {
            int[] sequence = [0, 1, 2, 3, 4, 7];
            var T9Equment = GameData.Instance.ItemEquipTable.Values
                .Where(item => item.ResourceId.EndsWith("_t9_1"))
                .Select(item => item.Id);
            foreach (int corp in sequence)
            {
                foreach (int tableItem in T9Equment)
                {
                    foreach (int i in Enumerable.Range(0, amount3))
                    {
                        user.Items.Add(new DbItemData
                        {
                            Isn = user.GenerateUniqueItemId(),
                            ItemType = tableItem,
                            Level = 5,
                            Exp = 0,
                            Count = 1,
                            Corp = corp
                        });
                    }
                    JsonDb.Save();
                }
            }
        }
        Console.WriteLine($"Added {amount1} of all FavoriteItem, {amount2} of all consumables, and {amount3} of all equipment to user " + user.ID);
        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse FinishAllTutorials(User user)
    {
        foreach (var tutorial in GameData.Instance.TutorialTable.Values)
        {
            if (!user.ClearedTutorialDataNew.ContainsKey(tutorial.GroupId))
            {
                user.ClearedTutorialDataNew.Add(tutorial.GroupId, new ClearedTutorialData()
                {
                    Id = tutorial.Id,
                    VersionGroup = tutorial.VersionGroup
                });
            }
            else
            {
                if (tutorial.Id > user.ClearedTutorialDataNew[tutorial.GroupId].Id)
                {
                    user.ClearedTutorialDataNew[tutorial.GroupId].Id = tutorial.Id;
                    user.ClearedTutorialDataNew[tutorial.GroupId].VersionGroup = tutorial.VersionGroup;
                }
            }
        }

        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse SetCoreLevel(User user, int inputGrade)
    {
        if (!(inputGrade >= 0 && inputGrade <= 11)) return new RunCmdResponse() { error = "core level out of range, must be between 0-12" };

        foreach (CharacterModel character in user.Characters)
        {
            // Get current character's Tid
            int tId = character.Tid;

            // Get the character data from the character table
            if (!GameData.Instance.CharacterTable.TryGetValue(tId, out CharacterRecord? charData))
            {
                Console.WriteLine($"Character data not found for Tid {tId}");
                continue;
            }

            int currentGradeCoreId = charData.GradeCoreId;
            int nameCode = charData.NameCode;
            var originalRare = charData.OriginalRare;

            // Skip characters with OriginalRare == "R"
            if (originalRare == OriginalRareType.R || nameCode == 3999)
            {
                continue;
            }

            // Now handle normal SR and SSR characters
            int maxGradeCoreId = 0;

            // If the character is "SSR", it can have a GradeCoreId from 1 to 11
            if (originalRare == OriginalRareType.SSR)
            {
                maxGradeCoreId = 11;  // SSR characters can go from 1 to 11

                // Calculate the new GradeCoreId within the bounds
                int newGradeCoreId = Math.Min(inputGrade + 1, maxGradeCoreId);  // +1 because inputGrade starts from 0 for SSRs

                // Find the character with the same NameCode and new GradeCoreId
                CharacterRecord? newCharData = GameData.Instance.CharacterTable.Values.FirstOrDefault(c =>
                    c.NameCode == nameCode && c.GradeCoreId == newGradeCoreId);

                if (newCharData != null)
                {
                    // Update the character's Tid and Grade
                    character.Tid = newCharData.Id;
                    character.Grade = newGradeCoreId;
                }

            }

            // If the character is "SR", it can have a GradeCoreId from 101 to 103
            else if (originalRare == OriginalRareType.SR)
            {
                maxGradeCoreId = 103;  // SR characters can go from 101 to 103

                // Start from 101 and increment based on inputGrade (inputGrade 0 -> GradeCoreId 101)
                int newGradeCoreId = Math.Min(101 + inputGrade, maxGradeCoreId);  // Starts at 101

                // Find the character with the same NameCode and new GradeCoreId
                CharacterRecord? newCharData = GameData.Instance.CharacterTable.Values.FirstOrDefault(c =>
                    c.NameCode == nameCode && c.GradeCoreId == newGradeCoreId);

                if (newCharData != null)
                {
                    // Update the character's Tid and Grade
                    character.Tid = newCharData.Id;
                    character.Grade = newGradeCoreId;
                }

            }
        }

        Console.WriteLine($"Core level of all characters have been set to {inputGrade}");
        JsonDb.Save();

        return RunCmdResponse.OK;
    }


    public static RunCmdResponse SetCharacterLevel(User user, int level)
    {
        if (level > 999 || level <= 0) return new RunCmdResponse() { error = "level must be between 1-999" };
        foreach (CharacterModel character in user.Characters)
        {
            character.Level = level;
        }
        Console.WriteLine("Set all characters' level to " + level);
        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse SetSkillLevel(User user, int skillLevel)
    {
        if (skillLevel > 10 || skillLevel < 0) return new RunCmdResponse() { error = "level must be between 1-10" };
        foreach (CharacterModel character in user.Characters)
        {
            character.UltimateLevel = skillLevel;
            character.Skill1Lvl = skillLevel;
            character.Skill2Lvl = skillLevel;
        }
        Console.WriteLine("Set all characters' skill levels to " + skillLevel);
        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse AddCharacter(User user, int characterId)
    {
        if (!user.HasCharacter(characterId))
        {
            user.Characters.Add(new CharacterModel()
            {
                CostumeId = 0,
                Csn = user.GenerateUniqueCharacterId(),
                Grade = 0,
                Level = 1,
                Skill1Lvl = 1,
                Skill2Lvl = 1,
                Tid = characterId,
                UltimateLevel = 1
            });

            if (GameData.Instance.CharacterTable.TryGetValue(characterId, out CharacterRecord? charData))
            {
                user.BondInfo.Add(new() { NameCode = charData.NameCode, Lv = 1 });
                user.AddTrigger(Trigger.ObtainCharacter, 1, charData.NameCode);
                user.AddTrigger(Trigger.ObtainCharacterNew, 1);
                if (charData.OriginalRare == OriginalRareType.SSR)
                    user.AddTrigger(Trigger.ObtainCharacterSSR, 1);
            }

            MessengerMessageCreator.CreateAllEligibleOpeners(user);

            Console.WriteLine($"Added character {characterId} to user");
            JsonDb.Save();
            return RunCmdResponse.OK;
        }
        else
        {
            return new RunCmdResponse() { error = $"User already has character {characterId}" };
        }
    }

    public static RunCmdResponse AddItem(User user, int itemId, int amount)
    {
        DbItemData? item = user.Items.FirstOrDefault(i => i.ItemType == itemId);

        if (item == null)
        {
            user.Items.Add(new DbItemData
            {
                Isn = user.GenerateUniqueItemId(),
                ItemType = itemId,
                Level = 1,
                Exp = 1,
                Count = amount
            });
        }
        else
        {
            item.Count += amount;

            if (item.Count < 0)
            {
                item.Count = 0;
            }
        }

        JsonDb.Save();
        return RunCmdResponse.OK;
    }

    public static RunCmdResponse SendMail(User user, int senderId, string title, string content, int validDays, List<MailAttachment> attachments)
    {
        try
        {
            // 创建邮件
            NetUserMailData mailData = new()
            {
                Sender = senderId,
                Msn =User.GenerateMsn(),
                CreatedAt = DateTime.Now.Ticks,
                HasReward = attachments != null && attachments.Count > 0,
                Nickname = "",
                Title = new() { IsPlain = true, Str = title },
                Text = new() { IsPlain = true, Str = content },
                State = 1,  // 1-未领取，2-已领取
                Type = 1,
                Period = validDays
            };
            // 添加附件
            if (attachments != null && attachments.Count > 0)
            {
                foreach (var att in attachments)
                {
                    mailData.Items.Add(new NetMailRewardItem()
                    {
                        ExpiredAt = DateTime.Now.AddDays(validDays).Ticks,
                        RewardId = att.Id,
                        RewardType = att.Type,
                        RewardValue = att.Count
                    });
                }
            }
            // 保存到用户邮件
            user.MailDatas.TryAdd(mailData.Msn, mailData);
            JsonDb.Save();

            return RunCmdResponse.OK;
        }
        catch (Exception ex)
        {
            return new RunCmdResponse { error = $"发送失败: {ex.Message}" };
        }
    }
    
    public static RunCmdResponse SkipTowerFloor(ulong userId, string args)
    {
        var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);

        try
        {
            var towerParsed = Enum.TryParse(args.Split('-')[0], out CorporationTowerType tower);
            var floorParsed = int.TryParse(args.Split('-')[1], out var floor);

            if (towerParsed && floorParsed)
            {
                Console.WriteLine($"Tower: {tower}, Floor: {floor} | ALL means Tribe Tower");

                var currentFloor = 0;
                if (user?.TowerProgress != null && user.TowerProgress.TryGetValue(tower, out var savedProgress))
                    currentFloor = savedProgress;

                if (floor <= currentFloor)
                    return new RunCmdResponse()
                    {
                        error = $"Invalid floor {floor} for tower {tower}. " +
                                $"Last Cleared {tower} Tower floor is {currentFloor}. " +
                                $"Input must be greater than {currentFloor}."
                    };

                var maxFloor = GameData.Instance.towerTable.Values
                    .Where(x => x.Type == tower)
                    .Select(x => x.Floor)
                    .DefaultIfEmpty(0)
                    .Max();

                if (maxFloor == 0)
                    return new RunCmdResponse() { error = $"No floors found for tower {tower}" };

                if (floor > maxFloor)
                    return new RunCmdResponse()
                    {
                        error = $"Invalid floor {floor} for tower {tower}. " +
                                $"Max floor is {maxFloor}. " +
                                $"Input must be between {currentFloor + 1} and {maxFloor}."
                    };

                if (currentFloor >= maxFloor)
                    return new RunCmdResponse()
                    {
                        error = $"Tower {tower} already fully cleared " +
                                $"(progress={currentFloor}, max={maxFloor})."
                    };

                var targetTower = GameData.Instance.towerTable.Values
                    .FirstOrDefault(x => x.Type == tower && x.Floor == floor);

                if (targetTower == null) return new RunCmdResponse() { error = $"Floor {floor} not found for tower {tower}" };

                var reward = TowerHelper.SkipTowerFloors(user, targetTower.Id, out var triggers);

                triggers.ForEach(trigger =>
                {
                    try
                    {
                        user.AddTrigger(trigger.Type, trigger.Value, trigger.ConditionId);
                    }
                    catch (ObjectDisposedException)
                    {
                        Console.WriteLine($"Warning: could not record trigger {trigger.Type} (conditionId {trigger.ConditionId}) - GameContext unavailable");
                    }
                });

                Console.WriteLine("Tower Clear Reward: " + JsonSerializer.Serialize(reward));
                JsonDb.Save();
            }
            else
                return new RunCmdResponse() { error = "Tower and floor number must be valid integers" };
        }
        catch (Exception ex)
        {
            return new RunCmdResponse() { error = "Exception: " + ex.ToString() };
        }
        return RunCmdResponse.OK;
    }
    
    internal static async Task<RunCmdResponse> UpdateResources()
    {
        Logging.WriteLine("updating static data and resource info...", LogType.Info);
        if (serverIp == null || staticDataUrl == null || resourcesUrl == null)
        {
            serverIp = await AssetDownloadUtil.GetIpAsync(serverUrl);
            staticDataUrl = $"https://{serverIp}/v1/get-static-data-pack-info-mpk";
            resourcesUrl = $"https://{serverIp}/v1/resourcehosts2";
        }

        if (serverIp == null)
            return new RunCmdResponse() { error = "failed to get real server ip, check internet connection" };

        // Get latest static data info from server
        ResStaticDataPackInfoMpk? staticData2 = await FetchProtobuf<ResStaticDataPackInfoMpk, ReqStaticDataPackInfoMpk>(staticDataUrl, new ReqStaticDataPackInfoMpk());
        if (staticData2 == null)
        {
            Logging.WriteLine("failed to fetch static data (2)", LogType.Error);
            return new RunCmdResponse() { error = "failed to fetch static data (2)" };
        }


        ResGetResourceHosts2? resources = await FetchProtobuf<ResGetResourceHosts2, ReqGetResourceHosts2>(resourcesUrl,
            new ReqGetResourceHosts2 { Version = GameConfig.Root.TargetVersion });
        if (resources == null)
        {
            Logging.WriteLine("failed to fetch resource data", LogType.Error);
            return new RunCmdResponse() { error = "failed to fetch resource data" };
        }

        GameConfig.Root.ResourceBaseURL = resources.BaseUrl;
        if (resources.DataPackVersionMap.TryGetValue(GameConfig.Root.TargetVersion, out var dataPackVersion)
            || resources.DataPackVersionMap.TryGetValue(resources.Version, out dataPackVersion))
            GameConfig.Root.ResourceDataPackVersion = dataPackVersion;
        GameConfig.Root.StaticDataMpk.Salt1 = staticData2.Salt1.ToBase64();
        GameConfig.Root.StaticDataMpk.Salt2 = staticData2.Salt2.ToBase64();
        GameConfig.Root.StaticDataMpk.Version = staticData2.Version;
        GameConfig.Root.StaticDataMpk.Url = staticData2.Url;
        GameConfig.Save();

        await GameData.CreateAsync();

        return RunCmdResponse.OK;
    }

    private static async Task<T?> FetchProtobuf<T, I>(string url, IMessage? input = null) where T : IMessage, new() where I : IMessage, new()
    {
        byte[] inputBytes = [];

        if (input != null)
        {
            using MemoryStream ms = new();
            CodedOutputStream stream = new(ms);
            input.WriteTo(stream);
            stream.Flush();

            inputBytes = ms.ToArray();
        }

        ByteArrayContent staticDataContent = new(inputBytes);
        client.DefaultRequestHeaders.Host = serverUrl;
        staticDataContent.Headers.Add("Content-Type", "application/octet-stream+protobuf");
        connectingServer = serverUrl;
        HttpResponseMessage? staticDataHttpResponse = await client.PostAsync(url, staticDataContent);
        if (staticDataHttpResponse == null)
        {
            Console.WriteLine($"failed to post {url}");
            return default;
        }

        if (!staticDataHttpResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"POST {url} failed with {staticDataHttpResponse.StatusCode}");
            return default;
        }

        byte[] staticDataHttpResponseBytes = await staticDataHttpResponse.Content.ReadAsByteArrayAsync();

        // Parse response
        T response = new();
        response.MergeFrom(new CodedInputStream(staticDataHttpResponseBytes));


        return response;
    }
}
