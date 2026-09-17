using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage;

[GameRequest("/stage/clearstage")]
public class ClearStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearStage req = await ReadData<ReqClearStage>();

        ResClearStage response = new();
        User user = GetUser();

        Console.WriteLine($"Stage " + req.StageId + " completed, result is " + req.BattleResult);

        // TODO: check if user has already cleared this stage
        if (req.BattleResult == 1)
        {
            response = CompleteStage(user, req.StageId);
        }

        await WriteDataAsync(response);
    }


    public static ResClearStage CompleteStage(User user, int StageId, bool forceCompleteScenarios = false)
    {
        ResClearStage response = new()
        {
            OutpostTimeRewardBuff = new()
        };
        CampaignStageRecord clearedStage = GameData.Instance.GetStageData(StageId) ?? throw new Exception("cleared stage cannot be null");

        string stageMapId = GameData.Instance.GetMapIdFromChapter(clearedStage.ChapterId, clearedStage.ChapterMod);

        if (user.FieldInfoNew.Count == 0)
        {
            user.FieldInfoNew.Add(stageMapId, new FieldInfoNew() { });
        }

        DoQuestSpecificUserOperations(user, StageId);
        RewardRecord? rewardData = GameData.Instance.GetRewardTableEntry(clearedStage.RewardId);

        if (forceCompleteScenarios)
        {
            if (!user.CompletedScenarios.Contains(clearedStage.EnterScenario) && !string.IsNullOrEmpty(clearedStage.EnterScenario) && !string.IsNullOrWhiteSpace(clearedStage.EnterScenario))
            {
                user.CompletedScenarios.Add(clearedStage.EnterScenario);
            }
            if (!user.CompletedScenarios.Contains(clearedStage.ExitScenario) && !string.IsNullOrEmpty(clearedStage.ExitScenario) && !string.IsNullOrWhiteSpace(clearedStage.ExitScenario))
            {
                user.CompletedScenarios.Add(clearedStage.ExitScenario);
            }
        }

        int oldLevel = user.userPointData.UserLevel;
        int oldOutpostLevel = user.OutpostBattleLevel.Level;

        if (rewardData != null)
            response.StageClearReward = RewardUtils.RegisterRewardsForUser(user, rewardData);
        else
            Console.WriteLine("rewardId is null for stage " + StageId);

        response.ScenarioReward = new NetRewardData() { PassPoint = new() };

        response.OutpostBattleLevelReward = new NetRewardData() { PassPoint = new() };

        // Check if user level changed, if so return the reward
        if (user.userPointData.UserLevel != oldLevel)
        {
            response.UserLevelUpReward = new NetRewardData();
            response.UserLevelUpReward.Currency.Add(new NetCurrencyData()
            {
                Type = (int)CurrencyType.FreeCash,
                Value = 30 * (user.userPointData.UserLevel - oldLevel),
                FinalValue = user.GetCurrencyVal(CurrencyType.FreeCash)
            });
        }
        // Check if outpost level changed, if so return the reward
        if (user.OutpostBattleLevel.Level != oldOutpostLevel)
        {
            response.OutpostBattleLevelReward = new NetRewardData();
            response.OutpostBattleLevelReward.Currency.Add(new NetCurrencyData()
            {
                Type = (int)CurrencyType.FreeCash,
                Value = 100 * (user.OutpostBattleLevel.Level - oldOutpostLevel),
                FinalValue = user.GetCurrencyVal(CurrencyType.FreeCash)
            });
        }

        if (clearedStage.StageCategory == StageCategory.Normal || clearedStage.StageCategory == StageCategory.Boss || clearedStage.StageCategory == StageCategory.Hard || clearedStage.StageCategory == StageCategory.Story)
        {
            if (clearedStage.ChapterMod == ChapterMod.Hard)
            {
                if (StageId > user.LastHardStageCleared)
                    user.LastHardStageCleared = StageId;
            }
            else if (clearedStage.ChapterMod == ChapterMod.Story)
            {
                if (StageId > user.LastStoryStageCleared)
                    user.LastStoryStageCleared = StageId;
            }
            else if (clearedStage.ChapterMod == ChapterMod.Normal)
            {
                if (StageId > user.LastNormalStageCleared)
                    user.LastNormalStageCleared = StageId;
            }
            else throw new NotImplementedException();
        }
        else
        {
            Logging.Warn("Unknown stage category " + clearedStage.StageCategory);
        }

        user.LastClearedDifficulty = (int)clearedStage.ChapterMod;

        if (clearedStage.StageType != StageType.Sub && clearedStage.ChapterMod != ChapterMod.Story)
        {
            // add outpost reward level if unlocked
            if (user.MainQuestData.TryGetValue(21, out bool _))
            {
                user.OutpostBattleLevel.Exp++;
                if (user.OutpostBattleLevel.Exp >= 5)
                {
                    user.OutpostBattleLevel.Exp = 0;
                    user.OutpostBattleLevel.Level++;
                    response.OutpostBattle = new NetOutpostBattleLevel() { IsLevelUp = true, Exp = 0, Level = user.OutpostBattleLevel.Level };
                    user.AddCurrency(CurrencyType.FreeCash, 100); // todo is reward the same for all level upgrades
                }
                else
                {
                    response.OutpostBattle = new NetOutpostBattleLevel() { IsLevelUp = false, Exp = user.OutpostBattleLevel.Exp, Level = user.OutpostBattleLevel.Level };
                }
            }
        }


        // Mark chapter as completed if boss stage was completed
        if (clearedStage.StageCategory == StageCategory.Boss && clearedStage.StageType == StageType.Main)
        {
            if (clearedStage.ChapterMod == ChapterMod.Hard)
                user.AddTrigger(Trigger.HardChapterClear, 1, clearedStage.ChapterId);
            else
                user.AddTrigger(Trigger.ChapterClear, 1, clearedStage.ChapterId);
        }

        if (!user.FieldInfoNew.ContainsKey(stageMapId))
            user.FieldInfoNew.Add(stageMapId, new FieldInfoNew());

        user.FieldInfoNew[stageMapId].CompletedStages.Add(StageId);

        // Subquests like Tetra Connect require clearing every stage of a stage
        // group, the client waits for this trigger to play the ending scenario.
        if (clearedStage.GroupId != 0)
        {
            using var ctx = GameContext.CreateNew();
            bool hasGroupClear = ctx.Triggers.Any(t => t.UserId == user.ID && t.Type == Trigger.CampaignGroupClear && t.ConditionId == clearedStage.GroupId);
            if (!hasGroupClear)
            {
                bool groupCleared = GameData.Instance.StageDataRecords.Values
                    .Where(s => s.GroupId == clearedStage.GroupId)
                    .All(s => user.FieldInfoNew.Values.Any(f => f.CompletedStages.Contains(s.Id)));

                if (groupCleared)
                    user.AddTrigger(Trigger.CampaignGroupClear, 1, clearedStage.GroupId);
            }
        }

        JsonDb.Save();
        return response;
    }

    private static void DoQuestSpecificUserOperations(User user, int clearedStageId)
    {
        MainQuestRecord? quest = GameData.Instance.GetMainQuestForStageClearCondition(clearedStageId);

        user.AddTrigger(Trigger.CampaignClear, 1, clearedStageId);
        if (quest != null)
        {
            user.SetQuest(quest.Id, false);
            user.AddTrigger(Trigger.MainQuestClear, 1, quest.Id);
        }
        else
        {
            // Some stages don't have quest records but are needed by messenger conditions.
            // Record MainQuestClear with the stageId as a fallback.
            bool neededByMessenger = GameData.Instance.MessageConditions.Values.Any(c =>
                c.TriggerList?.Any(t => t.Trigger == Trigger.MainQuestClear && t.ConditionId == clearedStageId) == true);
            if (neededByMessenger)
            {
                user.AddTrigger(Trigger.MainQuestClear, 1, clearedStageId);
            }
        }

        if (clearedStageId == 6000002 || clearedStageId == 6001004)
        {
            EnsureDefaultCharacters(user);
        }
    }

    public static void EnsureDefaultCharacters(User user)
    {
        if (user.Characters.Count > 0 && user.UserTeams.ContainsKey(1))
            return;

        if (user.Characters.Count == 0)
        {
            user.Characters.Add(new CharacterModel() { Csn = 47263455, Tid = 201001 });
            user.Characters.Add(new CharacterModel() { Csn = 47273456, Tid = 330501 });
            user.Characters.Add(new CharacterModel() { Csn = 47263457, Tid = 130201 });
            user.Characters.Add(new CharacterModel() { Csn = 47263458, Tid = 230101 });
            user.Characters.Add(new CharacterModel() { Csn = 47263459, Tid = 301201 });

            user.BondInfo.Add(new() { NameCode = 3001, Lv = 1 });
            user.BondInfo.Add(new() { NameCode = 3005, Lv = 1 });

            user.AddTrigger(Trigger.ObtainCharacter, 1, 3001);
            user.AddTrigger(Trigger.ObtainCharacter, 1, 1018);
            user.AddTrigger(Trigger.ObtainCharacter, 1, 1015);
            user.AddTrigger(Trigger.ObtainCharacter, 1, 1014);
            user.AddTrigger(Trigger.ObtainCharacter, 1, 3005);
            user.AddTrigger(Trigger.ObtainCharacterNew, 1);
        }

        if (!user.UserTeams.ContainsKey(1))
        {
            NetUserTeamData team1 = new()
            {
                Type = 1,
                LastContentsTeamNumber = 1
            };

            NetTeamData team1Sub = new()
            {
                TeamNumber = 1
            };

            for (int i = 1; i <= Math.Min(5, user.Characters.Count); i++)
            {
                CharacterModel character = user.Characters[i - 1];
                team1Sub.Slots.Add(new NetTeamSlot() { Slot = i, Value = character.Csn });
            }
            team1.Teams.Add(team1Sub);
            user.UserTeams[1] = team1;
        }

        if (user.RepresentationTeamDataNew.Length == 0 && user.Characters.Count >= 5)
        {
            user.RepresentationTeamDataNew =
            [
                user.Characters[0].Csn,
                user.Characters[1].Csn,
                user.Characters[2].Csn,
                user.Characters[3].Csn,
                user.Characters[4].Csn
            ];
        }
    }

    public static List<int> GetCompletedQuestsForStage(int targetStageId)
    {
        List<int> result = [];
        int currentId = 1;
        while (currentId != 0 && currentId != 9999)
        {
            if (!GameData.Instance.QuestDataRecords.TryGetValue(currentId, out var quest))
                break;

            int stageCondition = (quest.ConditionId != null && quest.ConditionId.Count > 0)
                ? quest.ConditionId[0].ConditionId
                : 0;

            if (quest.Category == Category.CampaignClear && stageCondition > targetStageId)
                break;

            result.Add(quest.Id);

            if (quest.Category == Category.CampaignClear && stageCondition == targetStageId)
                break;

            currentId = quest.NextMainQuestId;
        }
        return result;
    }

    public static void ReconcileMainQuests(User user)
    {
        if (user.LastNormalStageCleared == 0)
            return;

        List<int> validQuests = GetCompletedQuestsForStage(user.LastNormalStageCleared);
        if (validQuests.Count == 0)
            return;

        HashSet<int> validSet = [.. validQuests];

        // Remove any quests that are beyond the user's progress
        List<int> toRemove = user.MainQuestData.Keys.Where(k => !validSet.Contains(k)).ToList();
        foreach (int k in toRemove)
        {
            user.MainQuestData.Remove(k);
        }

        // Ensure all valid quests up to current progress are present
        foreach (int questId in validQuests)
        {
            user.MainQuestData.TryAdd(questId, false);
        }

        // Clean up any stale MainQuestClear triggers in database
        try
        {
            using (GameContext context = GameContext.CreateNew())
            {
                var badTriggers = context.Triggers
                    .Where(t => t.UserId == user.ID && t.Type == Trigger.MainQuestClear && t.ConditionId <= 9999 && !validSet.Contains(t.ConditionId))
                    .ToList();
                if (badTriggers.Count > 0)
                {
                    context.Triggers.RemoveRange(badTriggers);
                    context.SaveChanges();
                }
            }
        }
        catch (Exception ex)
        {
            Logging.Warn($"Failed to clean up stale triggers for user {user.ID}: {ex.Message}");
        }
    }
}

