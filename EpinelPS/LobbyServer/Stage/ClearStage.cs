using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/clearstage")]
    public class ClearStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearStage>();

            var response = new ResClearStage();
            var user = GetUser();

            Console.WriteLine($"Stage " + req.StageId + " completed, result is " + req.BattleResult);

            // TODO: check if user has already cleared this stage
            if (req.BattleResult == 1)
            {
                response = CompleteStage(user, req.StageId);
            }

            await WriteDataAsync(response);
        }


        public static ResClearStage CompleteStage(Database.User user, int StageId, bool forceCompleteScenarios = false)
        {
            var response = new ResClearStage();
            var clearedStage = GameData.Instance.GetStageData(StageId);
            if (clearedStage == null) throw new Exception("cleared stage cannot be null");


            if (user.FieldInfoNew.Count == 0)
            {
                user.FieldInfoNew.Add("0_" + clearedStage.chapter_mod, new FieldInfoNew() { });
            }

            DoQuestSpecificUserOperations(user, StageId);
            var rewardData = GameData.Instance.GetRewardTableEntry(clearedStage.reward_id);


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

            if (rewardData != null)
                response.StageClearReward = RewardUtils.RegisterRewardsForUser(user, rewardData);
            else
                Console.WriteLine("rewardId is null for stage " + StageId);


            if (clearedStage.stage_category == "Normal" || clearedStage.stage_category == "Boss" || clearedStage.stage_category == "Hard")
            {
                if (clearedStage.chapter_mod == "Hard")
                {
                    user.LastHardStageCleared = StageId;
                }
                else if (clearedStage.chapter_mod == "Normal")
                {
                    user.LastNormalStageCleared = StageId;
                }
                else
                {
                    Console.WriteLine("Unknown chapter mod " + clearedStage.chapter_mod);
                }
            }
            else if (clearedStage.stage_category == "Extra")
            {

            }
            else
            {
                Console.WriteLine("Unknown stage category " + clearedStage.stage_category);
            }

            if (clearedStage.stage_type != "Sub")
            {
                // add outpost reward level if unlocked
                if (user.MainQuestData.TryGetValue(21, out bool c))
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

            // CreateClearInfo(user);
            
            var key = (clearedStage.chapter_id - 1) + "_" + clearedStage.chapter_mod;
            if (!user.FieldInfoNew.ContainsKey(key))
                user.FieldInfoNew.Add(key, new FieldInfoNew());

            user.FieldInfoNew[key].CompletedStages.Add(StageId);
            JsonDb.Save();
            return response;
        }

        private static void DoQuestSpecificUserOperations(Database.User user, int clearedStageId)
        {
            var quest = GameData.Instance.GetMainQuestForStageClearCondition(clearedStageId);
            if (quest != null)
                user.SetQuest(quest.id, false);

            if (clearedStageId == 6000003)
            {
                // TODO: Is this the right place to copy over default characters?
                // TODO: What is CSN and TID? Also need to add names for these
                // Note: TID is table index, not sure what CSN is

                // create a squad with first 5 characters
                var team1 = new NetUserTeamData();
                team1.Type = 1;
                team1.LastContentsTeamNumber = 1;


                user.Characters.Add(new Database.Character() { Csn = 47263455, Tid = 201001 });
                user.Characters.Add(new Database.Character() { Csn = 47273456, Tid = 330501 });
                user.Characters.Add(new Database.Character() { Csn = 47263457, Tid = 130201 });
                user.Characters.Add(new Database.Character() { Csn = 47263458, Tid = 230101 });
                user.Characters.Add(new Database.Character() { Csn = 47263459, Tid = 301201 });

                var team1Sub = new NetTeamData();
                team1Sub.TeamNumber = 1;
                for (int i = 1; i < 6; i++)
                {
                    var character = user.Characters[i - 1];
                    team1Sub.Slots.Add(new NetTeamSlot() { Slot = i, Value = character.Csn });
                }
                team1.Teams.Add(team1Sub);
                user.UserTeams.Add(1, team1);


                user.RepresentationTeamData.TeamNumber = 1;
                user.RepresentationTeamData.TeamCombat = 1446; // TODO: Don't hardcode this
                user.RepresentationTeamData.Slots.Clear();
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 1, Csn = 47263455, Tid = 201001, Level = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 2, Csn = 47273456, Tid = 330501, Level = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 3, Csn = 47263457, Tid = 130201, Level = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 4, Csn = 47263458, Tid = 230101, Level = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 5, Csn = 47263459, Tid = 301201, Level = 1 });
            }
            // TODO: add neon
        }

        private static void CreateClearInfo(Database.User user)
        {
            NetStageClearInfo clearInfo = new NetStageClearInfo
            {
                User = LobbyHandler.CreateWholeUserDataFromDbUser(user),
                TeamCombat = user.RepresentationTeamData.TeamCombat,
                ClearedAt = DateTimeOffset.UtcNow.Ticks
            };

            foreach (var character in user.RepresentationTeamData.Slots)
            {
                clearInfo.Slots.Add(new NetStageClearInfoTeam()
                {
                    Slot = character.Slot,
                    Tid = character.Tid,
                    Level = character.Level,
                    Combat = FormulaUtils.CalculateCP(user, character.Csn),
                    CharacterType = StageClearInfoTeamCharacterType.StageClearInfoTeamCharacterTypeOwnedCharacter // TODO: how do we get this?
                });
            }

            user.StageClearHistorys.Add(clearInfo);
        }
    }
}
