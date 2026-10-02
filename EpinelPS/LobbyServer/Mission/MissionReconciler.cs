using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission;

/// <summary>
/// Reconciles mission progression and achievement triggers for users.
/// Ensures all earned milestones (character levels, user levels, limits, core breaks,
/// chapter clears, tower floors, and campaign quests) are properly recorded in the database
/// so they are completely achievable and claimable across all mission tabs.
/// </summary>
public static class MissionReconciler
{
    private static readonly object SyncLock = new();

    private static readonly Dictionary<CorporationTowerType, Trigger> TowerClearTriggers = new()
    {
        [CorporationTowerType.ELYSION] = Trigger.TowerElysionClear,
        [CorporationTowerType.MISSILIS] = Trigger.TowerMissilisClear,
        [CorporationTowerType.TETRA] = Trigger.TowerTetraClear,
        [CorporationTowerType.OVERSPEC] = Trigger.TowerOverspecClear,
        [CorporationTowerType.ALL] = Trigger.TowerBasicClear,
    };

    /// <summary>
    /// Evaluates account state and syncs any missing milestone triggers to the database.
    /// Safe to call frequently; checks existing triggers before writing to avoid redundant records.
    /// </summary>
    public static void ReconcileAll(User user, bool logToConsole = false)
    {
        if (user == null) return;

        lock (SyncLock)
        {
            try
            {
                using var context = GameContext.CreateNew();

                // 1. Reconcile maximum character level (Achievement: "Upgrade NIKKE to level 20... 40... 60")
                try { ReconcileCharacterLevelMax(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 1 CharLevel failed: {ex.Message}", LogType.Debug); }

                // 2. Reconcile player account level (Achievement: "Reach Account Level X")
                try { ReconcileUserLevel(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 2 UserLevel failed: {ex.Message}", LogType.Debug); }

                // 3. Reconcile character limit break and core break achievements
                try { ReconcileCharacterGradeAndCore(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 3 GradeAndCore failed: {ex.Message}", LogType.Debug); }

                // 4. Reconcile character bond / attractive levels
                try { ReconcileAttractiveLevelMax(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 4 Attractive failed: {ex.Message}", LogType.Debug); }

                // 5. Reconcile count of unique NIKKEs recruited
                try { ReconcileObtainCharacterNew(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 5 ObtainCharNew failed: {ex.Message}", LogType.Debug); }

                // 6. Reconcile New Commander Special Recruitment challenge
                try { ReconcileSpecialRecruitment(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 6 SpecialRecruit failed: {ex.Message}", LogType.Debug); }

                // 7. Reconcile "Complete XX Challenge(s)" milestone banner
                try { ReconcilePointRewardAchievement(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 7 PointRewardAchievement failed: {ex.Message}", LogType.Debug); }

                // 8. Auto-complete online & PvP challenges unavailable on private server
                try { ReconcileOnlineAndPvPFeatures(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 8 OnlineAndPvP failed: {ex.Message}", LogType.Debug); }

                // 9. Reconcile campaign chapter clears (Normal and Hard mode)
                try { ReconcileChapterClears(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 9 ChapterClears failed: {ex.Message}", LogType.Debug); }

                // 10. Reconcile Tribe Tower floor progression
                try { ReconcileTowerClears(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 10 TowerClears failed: {ex.Message}", LogType.Debug); }

                // 11. Reconcile Main Campaign Quests (when static game data is initialized)
                try
                {
                    if (GameData.Instance != null && GameData.Instance.QuestDataRecords != null)
                    {
                        ClearStage.ReconcileMainQuests(user, logToConsole);
                    }
                }
                catch (Exception qEx)
                {
                    Logging.WriteLine($"[MissionReconciler] Main quest reconciliation skipped: {qEx.Message}", LogType.Debug);
                }

                // 12. Reconcile beginner daily event progression (Day by Day, Alice's Diary)
                try { ReconcileDailyEventMissions(user, context, logToConsole); }
                catch (Exception ex) { Logging.WriteLine($"[MissionReconciler] Step 12 DailyEventMissions failed: {ex.Message}", LogType.Debug); }
            }
            catch (Exception ex)
            {
                Logging.Warn($"[MissionReconciler] Failed during reconciliation for user {user.ID}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Ensures that CharacterLevelMax is recorded with the highest level owned.
    /// This directly solves the issue where achievements like "Upgrade NIKKE to level 20... 40... 60"
    /// were stuck or blocked for existing high-level characters.
    /// </summary>
    private static void ReconcileCharacterLevelMax(User user, GameContext context, bool logToConsole)
    {
        int maxLevel = user.GetMaxCharacterLevel();
        if (maxLevel < 20) return;

        int existingMax = context.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.CharacterLevelMax)
            .Select(t => (int?)t.Value)
            .Max() ?? 0;

        if (existingMax < maxLevel)
        {
            user.AddTrigger(Trigger.CharacterLevelMax, maxLevel, 0, logToConsole);
            if (logToConsole)
            {
                Logging.WriteLine($"[MissionReconciler] Reconciled CharacterLevelMax to {maxLevel} for user {user.ID}", LogType.Info);
            }
        }
    }

    /// <summary>
    /// Reconciles player account level achievement triggers.
    /// </summary>
    private static void ReconcileUserLevel(User user, GameContext context, bool logToConsole)
    {
        int userLv = user.UserLevel > 0 ? user.UserLevel : 1;
        if (userLv < 10) return;

        int existingLevel = context.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.UserLevel)
            .Select(t => (int?)t.Value)
            .Max() ?? 0;

        if (existingLevel < userLv)
        {
            user.AddTrigger(Trigger.UserLevel, userLv, 0, logToConsole);
            if (logToConsole)
            {
                Logging.WriteLine($"[MissionReconciler] Reconciled UserLevel to {userLv} for user {user.ID}", LogType.Info);
            }
        }
    }

    /// <summary>
    /// Reconciles maximum limit break (stars) and core breaks across owned characters.
    /// Handles both raw grade numbers and GradeCoreIds (10..12 for SR, 100..103 for SSR, 201..207 for Core).
    /// </summary>
    private static void ReconcileCharacterGradeAndCore(User user, GameContext context, bool logToConsole)
    {
        if (user.Characters.Count == 0) return;

        int stars = 0;
        int core = 0;
        foreach (var c in user.Characters)
        {
            int charStars = 0;
            int charCore = 0;
            if (c.Grade >= 201 && c.Grade <= 207)
            {
                charStars = 3;
                charCore = c.Grade - 200;
            }
            else if (c.Grade >= 100 && c.Grade <= 103)
            {
                charStars = c.Grade - 100;
            }
            else if (c.Grade >= 10 && c.Grade <= 12)
            {
                charStars = c.Grade - 10;
            }
            else if (c.Grade > 3 && c.Grade <= 10)
            {
                charStars = 3;
                charCore = c.Grade - 3;
            }
            else if (c.Grade >= 0 && c.Grade <= 3)
            {
                charStars = c.Grade;
            }

            stars = Math.Max(stars, charStars);
            core = Math.Max(core, charCore);
        }

        // Grade 2 (SR max limit break) and Grade 3 (SSR max limit break)
        if (stars >= 2)
        {
            int targetGrade = Math.Min(stars, 3);
            int existingGrade = context.Triggers
                .Where(t => t.UserId == user.ID && t.Type == Trigger.CharacterGradeMax)
                .Select(t => (int?)t.Value)
                .Max() ?? 0;

            if (existingGrade < targetGrade)
            {
                user.AddTrigger(Trigger.CharacterGradeMax, targetGrade, 0, logToConsole);
            }
        }

        // Core breaks (1..7)
        if (core > 0)
        {
            int targetCore = Math.Min(core, 7);
            int existingCore = context.Triggers
                .Where(t => t.UserId == user.ID && t.Type == Trigger.CharacterCore)
                .Select(t => (int?)t.Value)
                .Max() ?? 0;

            if (existingCore < targetCore)
            {
                user.AddTrigger(Trigger.CharacterCore, targetCore, 0, logToConsole);
            }
        }
    }

    /// <summary>
    /// Reconciles character bond (attractive) level achievements.
    /// </summary>
    private static void ReconcileAttractiveLevelMax(User user, GameContext context, bool logToConsole)
    {
        if (user.BondInfo == null || user.BondInfo.Count == 0) return;

        int maxBond = user.BondInfo.Max(b => b.Lv);
        if (maxBond < 2) return;

        int existingBond = context.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.CharacterAttractiveLevelMax && t.ConditionId == 0)
            .Select(t => (int?)t.Value)
            .Max() ?? 0;

        if (existingBond < maxBond)
        {
            user.AddTrigger(Trigger.CharacterAttractiveLevelMax, maxBond, 0, logToConsole);
        }
    }

    /// <summary>
    /// Reconciles the "Recruit Nikkes in New Commander Special Recruitment" challenge (Trigger: FirstPaidGachaLegacy).
    /// </summary>
    private static void ReconcileSpecialRecruitment(User user, GameContext context, bool logToConsole)
    {
        // Banner ID 4 is NEW_PLAYER_SPECIAL_BANNER_ID
        bool hasPulledSpecial = user.GachaBannerMaxPulls.TryGetValue(4, out int pulls) && pulls > 0;
        if (!hasPulledSpecial) return;

        bool hasTrigger = context.Triggers.Any(t => t.UserId == user.ID && t.Type == Trigger.FirstPaidGachaLegacy);
        if (!hasTrigger)
        {
            user.AddTrigger(Trigger.FirstPaidGachaLegacy, 1, 0, logToConsole);
            if (logToConsole)
            {
                Logging.WriteLine($"[MissionReconciler] Reconciled FirstPaidGachaLegacy for user {user.ID}", LogType.Info);
            }
        }
    }

    /// <summary>
    /// Reconciles total count of unique NIKKEs acquired.
    /// Ensures trigger sum strictly matches the player's unique roster count, fixing the bug
    /// where multiple reconciliation/pull records stacked and caused the challenge to skip to "Recruit 70 Nikke(s)".
    /// Also sanitizes any premature achievements claimed above the user's actual roster count.
    /// </summary>
    private static void ReconcileObtainCharacterNew(User user, GameContext context, bool logToConsole)
    {
        if (user.Characters.Count == 0) return;

        int uniqueCount;
        if (GameData.Instance != null && GameData.Instance.CharacterTable != null)
        {
            uniqueCount = user.Characters
                .Select(c => GameData.Instance.CharacterTable.TryGetValue(c.Tid, out var rec) ? rec.NameCode : 0)
                .Where(nc => nc > 0)
                .Distinct()
                .Count();
        }
        else
        {
            uniqueCount = user.Characters.Select(c => c.Tid).Distinct().Count();
        }

        var ocnTriggers = context.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.ObtainCharacterNew)
            .ToList();

        int currentSum = ocnTriggers.Sum(t => t.Value);

        if (currentSum > uniqueCount)
        {
            // Triggers were stacked/overcounted from previous passes or pull events
            context.Triggers.RemoveRange(ocnTriggers);
            context.SaveChanges();

            if (uniqueCount > 0)
            {
                user.AddTrigger(Trigger.ObtainCharacterNew, uniqueCount, 0, logToConsole);
            }
            user.NeedsTriggerSyncRestart = true;
            if (logToConsole)
            {
                Logging.WriteLine($"[MissionReconciler] Corrected ObtainCharacterNew sum (was {currentSum}, reset to {uniqueCount}) for user {user.ID}", LogType.Info);
            }
        }
        else if (currentSum < uniqueCount)
        {
            user.AddTrigger(Trigger.ObtainCharacterNew, uniqueCount - currentSum, 0, logToConsole);
        }

        // Sanitize any prematurely claimed ObtainCharacterNew achievements above the user's actual roster count
        if (GameData.Instance != null && GameData.Instance.TriggerTable != null)
        {
            int removedCount = user.CompletedAchievements.RemoveAll(id => 
                GameData.Instance.TriggerTable.TryGetValue(id, out var t) && 
                t.Trigger == Trigger.ObtainCharacterNew && 
                t.ConditionValue > uniqueCount);

            if (removedCount > 0)
            {
                JsonDb.Save();
                if (logToConsole)
                {
                    Logging.WriteLine($"[MissionReconciler] Cleaned up {removedCount} prematurely claimed recruit achievements above count {uniqueCount}", LogType.Info);
                }
            }
        }
    }

    /// <summary>
    /// Reconciles the "Complete XX Challenge(s)" milestone banner (Trigger: PointRewardAchievement).
    /// Each completed regular challenge awards 1 milestone point.
    /// Fixes the critical bug where PointValue (10,000+) was added to PointRewardAchievement
    /// and emptying the banner prematurely.
    /// </summary>
    private static void ReconcilePointRewardAchievement(User user, GameContext context, bool logToConsole)
    {
        if (GameData.Instance == null || GameData.Instance.TriggerTable == null) return;

        // Count how many regular (non-milestone) challenges have been completed
        int actualCompletedChallenges = user.CompletedAchievements
            .Count(id => GameData.Instance.TriggerTable.TryGetValue(id, out var t) && t.Trigger != Trigger.PointRewardAchievement);

        var praTriggers = context.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.PointRewardAchievement)
            .ToList();

        int currentPraSum = praTriggers.Sum(t => t.Value);

        if (currentPraSum > actualCompletedChallenges)
        {
            // Corrupted/inflated point triggers found (e.g. 105,750 from old bug)
            context.Triggers.RemoveRange(praTriggers);
            context.SaveChanges();

            if (actualCompletedChallenges > 0)
            {
                user.AddTrigger(Trigger.PointRewardAchievement, actualCompletedChallenges, 0, logToConsole);
            }
            user.NeedsTriggerSyncRestart = true;
            if (logToConsole)
            {
                Logging.WriteLine($"[MissionReconciler] Fixed corrupted PointRewardAchievement (was {currentPraSum}, reset to {actualCompletedChallenges}) for user {user.ID}", LogType.Info);
            }
        }
        else if (currentPraSum < actualCompletedChallenges)
        {
            user.AddTrigger(Trigger.PointRewardAchievement, actualCompletedChallenges - currentPraSum, 0, logToConsole);
        }

        // Sanitize any prematurely claimed milestone chests above actual completed challenge count
        int removedMilestones = user.CompletedAchievements.RemoveAll(id =>
            GameData.Instance.TriggerTable.TryGetValue(id, out var t) &&
            t.Trigger == Trigger.PointRewardAchievement &&
            t.ConditionValue > actualCompletedChallenges);

        if (removedMilestones > 0)
        {
            JsonDb.Save();
            if (logToConsole)
            {
                Logging.WriteLine($"[MissionReconciler] Cleaned up {removedMilestones} prematurely claimed challenge milestone chests above count {actualCompletedChallenges}", LogType.Info);
            }
        }
    }

    /// <summary>
    /// Auto-completes challenge and mission achievements that depend on live networking features
    /// not present in a local private server (Friendship points exchange, Rookie Arena wins, Champion Arena season cheering).
    /// </summary>
    private static void ReconcileOnlineAndPvPFeatures(User user, GameContext context, bool logToConsole)
    {
        // 1. Send Social Points (SendFriendShipPoint, up to max challenge tier 57,000)
        const int maxFriendshipPoints = 57000;
        int currentFp = context.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.SendFriendShipPoint)
            .Sum(t => (int?)t.Value) ?? 0;
        if (currentFp < maxFriendshipPoints)
        {
            user.AddTrigger(Trigger.SendFriendShipPoint, maxFriendshipPoints - currentFp, 0, logToConsole);
            if (logToConsole) Logging.WriteLine($"[MissionReconciler] Auto-completed SendFriendShipPoint ({maxFriendshipPoints}) for user {user.ID}", LogType.Info);
        }

        // 2. Rookie Arena Wins (WinArena, up to max challenge tier 10,000)
        const int maxArenaWins = 10000;
        int currentWins = context.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.WinArena)
            .Sum(t => (int?)t.Value) ?? 0;
        if (currentWins < maxArenaWins)
        {
            user.AddTrigger(Trigger.WinArena, maxArenaWins - currentWins, 0, logToConsole);
            if (logToConsole) Logging.WriteLine($"[MissionReconciler] Auto-completed WinArena ({maxArenaWins}) for user {user.ID}", LogType.Info);
        }

        // 3. Rookie Arena Play Count (Daily/Weekly missions: 2 and 10 plays; Alice's Diary: up to 25 plays)
        const int maxArenaPlays = 25;
        int currentPlays = context.Triggers
            .Where(t => t.UserId == user.ID && t.Type == Trigger.RookieArenaPlayCount)
            .Sum(t => (int?)t.Value) ?? 0;
        if (currentPlays < maxArenaPlays)
        {
            user.AddTrigger(Trigger.RookieArenaPlayCount, maxArenaPlays - currentPlays, 0, logToConsole);
        }

        // 4. Champion Arena season cheering achievements (Win and Lose all gambles in one season)
        if (!context.Triggers.Any(t => t.UserId == user.ID && t.Type == Trigger.ChampionArenaGambleWinAll))
        {
            user.AddTrigger(Trigger.ChampionArenaGambleWinAll, 1, 0, logToConsole);
        }
        if (!context.Triggers.Any(t => t.UserId == user.ID && t.Type == Trigger.ChampionArenaGambleLoseAll))
        {
            user.AddTrigger(Trigger.ChampionArenaGambleLoseAll, 1, 0, logToConsole);
        }

        // 5. Tactics Academy 9-4 completion for Alice's Diary Day 1
        if (user.CompletedTacticAcademyLessons.Contains(9004))
        {
            if (!context.Triggers.Any(t => t.UserId == user.ID && t.Type == Trigger.TacticAcademyFinish94))
            {
                user.AddTrigger(Trigger.TacticAcademyFinish94, 1, 0, logToConsole);
                if (logToConsole) Logging.WriteLine($"[MissionReconciler] Reconciled TacticAcademyFinish94 for user {user.ID}", LogType.Info);
            }
        }
    }

    /// <summary>
    /// Reconciles trigger progress for beginner daily events (Day by Day and Alice's Diary).
    /// Ensures that Trigger.EventPoint (advancing the 0/45 and 0/70 progress bar) and
    /// Trigger.DailyEventClear (advancing Day 1..14 main mission completion) stay strictly
    /// consistent with the user's claimed mission records.
    /// </summary>
    private static void ReconcileDailyEventMissions(User user, GameContext context, bool logToConsole)
    {
        if (GameData.Instance == null || GameData.Instance.DailyEventTable == null) return;
        if (user.EventMissionInfo == null || user.EventMissionInfo.Count == 0) return;

        foreach (var (eventId, userEvent) in user.EventMissionInfo)
        {
            if (userEvent.MissionIdList == null || userEvent.MissionIdList.Count == 0) continue;

            // Only process beginner daily events (e.g. 20001, 20002)
            if (eventId != 20001 && eventId != 20002) continue;

            int expectedEventPoints = 0;
            Dictionary<int, int> expectedGroupClears = [];

            foreach (var claimedId in userEvent.MissionIdList)
            {
                if (GameData.Instance.DailyEventTable.TryGetValue(claimedId, out var dailyRecord))
                {
                    if (!dailyRecord.IsMain)
                    {
                        expectedEventPoints += dailyRecord.PointValue > 0 ? dailyRecord.PointValue : 1;
                        if (dailyRecord.EventPhaseGroupId > 0)
                        {
                            expectedGroupClears[dailyRecord.EventPhaseGroupId] = expectedGroupClears.GetValueOrDefault(dailyRecord.EventPhaseGroupId, 0) + 1;
                        }
                    }
                }
            }

            // Check Trigger.EventPoint for this event
            int currentEventPoints = context.Triggers
                .Where(t => t.UserId == user.ID && t.Type == Trigger.EventPoint && t.ConditionId == eventId)
                .Sum(t => (int?)t.Value) ?? 0;

            if (currentEventPoints < expectedEventPoints)
            {
                user.AddTrigger(Trigger.EventPoint, expectedEventPoints - currentEventPoints, eventId, logToConsole);
                if (logToConsole)
                {
                    Logging.WriteLine($"[MissionReconciler] Reconciled EventPoint for event {eventId} (was {currentEventPoints}, added {expectedEventPoints - currentEventPoints}) for user {user.ID}", LogType.Info);
                }
            }

            // Check Trigger.DailyEventClear for each phase group
            foreach (var (groupId, expectedClears) in expectedGroupClears)
            {
                int currentClears = context.Triggers
                    .Where(t => t.UserId == user.ID && t.Type == Trigger.DailyEventClear && t.ConditionId == groupId)
                    .Sum(t => (int?)t.Value) ?? 0;

                if (currentClears < expectedClears)
                {
                    user.AddTrigger(Trigger.DailyEventClear, expectedClears - currentClears, groupId, logToConsole);
                }
            }
        }
    }

    /// <summary>
    /// Reconciles normal and hard chapter clear achievements.
    /// </summary>
    private static void ReconcileChapterClears(User user, GameContext context, bool logToConsole)
    {
        if (GameData.Instance == null || GameData.Instance.ChapterCampaignData == null) return;

        // Reconcile HardChapterClear triggers if the player has cleared Hard stages
        if (user.LastHardStageCleared > 0)
        {
            var existingHardChapters = context.Triggers
                .Where(t => t.UserId == user.ID && t.Type == Trigger.HardChapterClear)
                .Select(t => t.ConditionId)
                .ToHashSet();

            var hardChapters = GameData.Instance.ChapterCampaignData.Values;
            int maxChapter = hardChapters.Count > 0 ? hardChapters.Max(c => c.Chapter) : 0;

            for (int c = 1; c <= maxChapter; c++)
            {
                var hardStages = AdminCommands.GetHardMainStages(c);
                var bossStage = hardStages.LastOrDefault();
                if (bossStage != null && user.IsStageCompleted(bossStage.Id))
                {
                    if (!existingHardChapters.Contains(bossStage.ChapterId))
                    {
                        user.AddTrigger(Trigger.HardChapterClear, 1, bossStage.ChapterId, logToConsole);
                        existingHardChapters.Add(bossStage.ChapterId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reconciles Tribe Tower floor achievements across all manufacturer towers.
    /// </summary>
    private static void ReconcileTowerClears(User user, GameContext context, bool logToConsole)
    {
        if (user.TowerProgress.Count == 0 || GameData.Instance == null || GameData.Instance.towerTable == null) return;

        foreach (var (corpType, clearedFloor) in user.TowerProgress)
        {
            if (clearedFloor < 5) continue;
            if (!TowerClearTriggers.TryGetValue(corpType, out Trigger triggerType)) continue;

            var existingFloors = context.Triggers
                .Where(t => t.UserId == user.ID && t.Type == triggerType)
                .Select(t => t.ConditionId)
                .ToHashSet();

            // Check each milestone floor (multiples of 5) up to current cleared floor
            var towerStages = GameData.Instance.towerTable.Values
                .Where(t => t.Type == corpType && t.Floor <= clearedFloor && t.Floor % 5 == 0)
                .OrderBy(t => t.Floor);

            foreach (var stage in towerStages)
            {
                if (!existingFloors.Contains(stage.Id))
                {
                    user.AddTrigger(triggerType, 1, stage.Id, logToConsole);
                    existingFloors.Add(stage.Id);
                }
            }
        }
    }
}
