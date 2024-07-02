using nksrv.StaticInfo;
using nksrv.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Stage
{
    [PacketPath("/stage/clearstage")]
    public class ClearStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearStage>();

            var response = new ResClearStage();
            var user = GetUser();

            // TOOD: save to user info
            Console.WriteLine($"Stage " + req.StageId + " completed, result is " + req.BattleResult);

            if (req.BattleResult == 1)
            {
                var clearedStage = StaticDataParser.Instance.GetStageData(req.StageId);
                if (clearedStage == null) throw new Exception("cleared stage cannot be null");

                user.LastNormalStageCleared = req.StageId;

                if (user.FieldInfo.Count == 0)
                {
                    user.FieldInfo.Add(0, new FieldInfo() { });
                }

                DoQuestSpecificUserOperations(user, req.StageId);
                var rewardData = StaticDataParser.Instance.GetRewardTableEntry(clearedStage.reward_id);

                user.FieldInfo[clearedStage.chapter_id - 1].CompletedStages.Add(new NetFieldStageData() { StageId = req.StageId });
                if (rewardData != null)
                    response.Reward = RegisterRewardsForUser(user, rewardData);
                else
                    Logger.Warn("rewardId is null for stage " + req.StageId);
                JsonDb.Save();
            }

            WriteData(response);
        }

        private NetRewardData RegisterRewardsForUser(Utils.User user, RewardTableRecord rewardData)
        {
            NetRewardData ret = new();
            if (rewardData.rewards == null) return ret;

            if (rewardData.user_exp != 0)
            {
                var newXp = rewardData.character_exp + user.userPointData.ExperiencePoint;
                var newLevel = StaticDataParser.Instance.GetUserLevelFromUserExp(newXp);
                if (newLevel == -1)
                {
                    Logger.Warn("Unknown user level value for xp " + newXp);
                }
                //ret.UserExp = new NetIncreaseExpData()
                //{
                //    BeforeExp = user.userPointData.ExperiencePoint,
                //    BeforeLv = user.userPointData.UserLevel,
                //    IncreaseExp = rewardData.character_exp,
                //    CurrentExp = rewardData.character_exp + newXp,
                //    CurrentLv = newLevel,
                //    GainExp = rewardData.character_exp
                //};
                user.userPointData.ExperiencePoint += rewardData.character_exp;
            }

            foreach (var item in rewardData.rewards)
            {
                if (item.reward_id != 0)
                {
                    if (string.IsNullOrEmpty(item.reward_type)) { }
                    else if (item.reward_type == "Currency")
                    {
                        Dictionary<CurrencyType, int> current = new Dictionary<CurrencyType, int>();

                        // add all currencies that users has to current dictionary
                        foreach (var currentReward in user.Currency)
                        {
                            if (!current.ContainsKey(currentReward.Key))
                                current.Add(currentReward.Key, 0);

                            current[currentReward.Key] = (int)currentReward.Value;
                        }

                        // add currency reward to response
                        CurrencyType t = (CurrencyType)item.reward_id;
                        int val = item.reward_value;
                        if (!current.ContainsKey(t))
                            current.Add(t, 0);
                        var val2 = current[t];
                        ret.Currency.Add(new NetCurrencyData() { Type = (int)t, Value = val, FinalValue = val2 + val });


                        // add currency reward to user info
                        if (!user.Currency.ContainsKey(t))
                            user.Currency.Add(t, val);
                        else
                            user.Currency[t] += val;
                    }
                }
                else
                {
                    Logger.Warn("TODO: Reward type " + item.reward_type);
                }
            }

            return ret;
        }

        private static void DoQuestSpecificUserOperations(Utils.User user, int clearedStageId)
        {
            var quest = StaticDataParser.Instance.GetMainQuestForStageClearCondition(clearedStageId);
            if (quest != null)
                user.SetQuest(quest.id, true);

            if (clearedStageId == 6000003)
            {
                // TODO: Is this the right place to copy over default characters?
                // TODO: What is CSN and TID? Also need to add names for these
                // Note: TID is table index, not sure what CSN is

                // create a squad with first 5 characters
                var team1 = new NetUserTeamData();
                team1.Type = 1;
                team1.LastContentsTeamNumber = 1;

                var team1Sub = new NetTeamData();
                team1Sub.TeamNumber = 1;
                for (int i = 1; i < 6; i++)
                {
                    var character = user.Characters[i - 1];
                    team1Sub.Slots.Add(new NetTeamSlot() { Slot = i, Value = character.Csn });
                }
                team1.Teams.Add(team1Sub);
                user.UserTeams.Add(1, team1);

                user.Characters.Add(new Utils.Character() { Csn = 47263455, Tid = 201001 });
                user.Characters.Add(new Utils.Character() { Csn = 47273456, Tid = 330501 });
                user.Characters.Add(new Utils.Character() { Csn = 47263457, Tid = 130201 });
                user.Characters.Add(new Utils.Character() { Csn = 47263458, Tid = 230101 });
                user.Characters.Add(new Utils.Character() { Csn = 47263459, Tid = 301201 });

                user.RepresentationTeamData.TeamNumber = 1;
                user.RepresentationTeamData.TeamCombat = 1446; // TODO: Don't hardcode this
                user.RepresentationTeamData.Slots.Clear();
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 1, Csn = 47263455, Tid = 201001, Lvl = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 2, Csn = 47273456, Tid = 330501, Lvl = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 3, Csn = 47263457, Tid = 130201, Lvl = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 4, Csn = 47263458, Tid = 230101, Lvl = 1 });
                user.RepresentationTeamData.Slots.Add(new NetWholeTeamSlot { Slot = 5, Csn = 47263459, Tid = 301201, Lvl = 1 });
            }
            // TODO: add neon
        }
    }
}
