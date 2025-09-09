using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/clearstage")]
    public class ClearStage : LobbyMsgHandler
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

            string stageMapId = GameData.Instance.GetMapIdFromChapter(clearedStage.chapter_id, clearedStage.chapter_mod);
            
            if (user.FieldInfoNew.Count == 0)
            {
                user.FieldInfoNew.Add(stageMapId, new FieldInfoNew() { });
            }

            DoQuestSpecificUserOperations(user, StageId);
            RewardTableRecord? rewardData = GameData.Instance.GetRewardTableEntry(clearedStage.reward_id);

            if (forceCompleteScenarios)
            {
                if (!user.CompletedScenarios.Contains(clearedStage.enter_scenario) && !string.IsNullOrEmpty(clearedStage.enter_scenario) && !string.IsNullOrWhiteSpace(clearedStage.enter_scenario))
                {
                    user.CompletedScenarios.Add(clearedStage.enter_scenario);
                }
                if (!user.CompletedScenarios.Contains(clearedStage.exit_scenario) && !string.IsNullOrEmpty(clearedStage.exit_scenario) && !string.IsNullOrWhiteSpace(clearedStage.exit_scenario))
                {
                    user.CompletedScenarios.Add(clearedStage.exit_scenario);
                }
            }

            int oldLevel = user.userPointData.UserLevel;
            int oldOutpostLevel = user.OutpostBattleLevel.Level;

            if (rewardData != null)
                response.StageClearReward = RewardUtils.RegisterRewardsForUser(user, rewardData);
            else
                Console.WriteLine("rewardId is null for stage " + StageId);
            response.Reward = response.StageClearReward;

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

            if (clearedStage.stage_category == StageCategory.Normal || clearedStage.stage_category == StageCategory.Boss || clearedStage.stage_category == StageCategory.Hard)
            {
                if (clearedStage.chapter_mod == ChapterMod.Hard)
                {
                    if (StageId > user.LastHardStageCleared)
                        user.LastHardStageCleared = StageId;
                }
                else
                {
                    if (StageId > user.LastNormalStageCleared)
                        user.LastNormalStageCleared = StageId;
                }
            }
            else
            {
                Logging.Warn("Unknown stage category " + clearedStage.stage_category);
            }

            if (clearedStage.stage_type != StageType.Sub)
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
            if (clearedStage.stage_category == StageCategory.Boss && clearedStage.stage_type == StageType.Main)
            {
                if (clearedStage.chapter_mod == ChapterMod.Hard)
                    user.AddTrigger(TriggerType.HardChapterClear, 1, clearedStage.chapter_id);
                else
                    user.AddTrigger(TriggerType.ChapterClear, 1, clearedStage.chapter_id);
            }

            if (!user.FieldInfoNew.ContainsKey(stageMapId))
                user.FieldInfoNew.Add(stageMapId, new FieldInfoNew());

            user.FieldInfoNew[stageMapId].CompletedStages.Add(StageId);
            JsonDb.Save();
            return response;
        }

        private static void DoQuestSpecificUserOperations(User user, int clearedStageId)
        {
            MainQuestCompletionRecord? quest = GameData.Instance.GetMainQuestForStageClearCondition(clearedStageId);

            user.AddTrigger(TriggerType.CampaignClear, 1, clearedStageId);
            if (quest != null)
            {
                user.SetQuest(quest.id, false);
                user.AddTrigger(TriggerType.MainQuestClear, 1, quest.id);
            }

            // TODO: Is this the right place to add default characters?
            // Stage 1-4 BOSS
            if (clearedStageId == 6001004)
            {
                // TID: Character ID
                // CSN: Character Serial Number

                // create a squad with first 5 characters
                NetUserTeamData team1 = new()
                {
                    Type = 1,
                    LastContentsTeamNumber = 1
                };

                user.Characters.Add(new CharacterModel() { Csn = 47263455, Tid = 201001 });
                user.Characters.Add(new CharacterModel() { Csn = 47273456, Tid = 330501 });
                user.Characters.Add(new CharacterModel() { Csn = 47263457, Tid = 130201 });
                user.Characters.Add(new CharacterModel() { Csn = 47263458, Tid = 230101 });
                user.Characters.Add(new CharacterModel() { Csn = 47263459, Tid = 301201 });

                user.BondInfo.Add(new() { NameCode = 3001, Lv = 1 });
                user.BondInfo.Add(new() { NameCode = 3005, Lv = 1 });

                user.AddTrigger(TriggerType.ObtainCharacter, 1, 3001);
                user.AddTrigger(TriggerType.ObtainCharacter, 1, 1018);
                user.AddTrigger(TriggerType.ObtainCharacter, 1, 1015);
                user.AddTrigger(TriggerType.ObtainCharacter, 1, 1014);
                user.AddTrigger(TriggerType.ObtainCharacter, 1, 3005);
                user.AddTrigger(TriggerType.ObtainCharacterNew, 1);

                NetTeamData team1Sub = new()
                {
                    TeamNumber = 1
                };

                for (int i = 1; i < 6; i++)
                {
                    CharacterModel character = user.Characters[i - 1];
                    team1Sub.Slots.Add(new NetTeamSlot() { Slot = i, Value = character.Csn });
                }
                team1.Teams.Add(team1Sub);
                user.UserTeams.Add(1, team1);

                user.RepresentationTeamDataNew =
                [
                    47263455,
                    47273456,
                    47263457,
                    47263458,
                    47263459
                ];
            }
        }
    }
}
