using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Services;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for stage completion
/// </summary>
[ApiController]
public class StageController(IUserService UserController, GameContext db, IInventoryService InventoryService) : Controller
{
    public static NetFieldObjectData CreateFieldInfo(GameUser user, string mapId, out bool BossEntered)
    {
        NetFieldObjectData f = new();
        bool found = false;
        BossEntered = false;

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == mapId);

        if (field == null)
        {
            field = new FieldInfo
            {
                MapName = mapId
            };
            user.FieldInfo.Add(field);
        }

        foreach (int stage in field.CompletedStages)
        {
            f.Stages.Add(new NetFieldStageData() { StageId = stage });
        }
        foreach (var obj in field.CompletedObjects)
        {
            f.Objects.Add(new NetFieldObject()
            {
                PositionId = obj.PositionId,
                ActionAt = obj.ActionAt.Ticks,
                Json = obj.Json,
                Type = obj.Type
            });
        }
        BossEntered = field.BossEntered;

        return f;
    }

    [Route("/v1/stage/get")]
    [HttpPost]
    public ActionResult<ResGetStageData> GetStageInfo([FromBodyProtobuf] ReqGetStageData req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        string mapId = GameData.Instance.GetMapIdFromChapter(req.Chapter, (ChapterMod)req.Mod);

        return new ResGetStageData()
        {
            Field = CreateFieldInfo(user, mapId, out bool bossEntered),
            HasChapterBossEntered = bossEntered,
            SquadData = ""
        };
    }

    [Route("/v1/stage/checkclear")]
    [HttpPost]
    public ActionResult<ResCheckStageClear> CheckCleared([FromBodyProtobuf] ReqCheckStageClear req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResCheckStageClear response = new();

        foreach (var fields in user.FieldInfo)
        {
            foreach (int stages in fields.CompletedStages)
            {
                if (req.StageIds.Contains(stages))
                    response.ClearedStageIds.Add(stages);
            }
        }

        return response;
    }

    [Route("/v1/stage/enterstage")]
    [HttpPost]
    public ActionResult<ResEnterStage> GetStageInfo([FromBodyProtobuf] ReqEnterStage req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        CampaignStageRecord clearedStage = GameData.Instance.GetStageData(req.StageId) ?? throw new Exception("cleared stage cannot be null");
        string map = GameData.Instance.GetMapIdFromChapter(clearedStage.ChapterId, clearedStage.ChapterMod);

        if (clearedStage.StageCategory == StageCategory.Boss)
        {
            var field = user.FieldInfo.FirstOrDefault(f => f.MapName == map);

            if (field == null)
            {
                field = new FieldInfo
                {
                    MapName = map
                };
                user.FieldInfo.Add(field);
            }

            field.BossEntered = true;
        }

        user.AddTrigger(Trigger.CampaignStart, 1, req.StageId);

        return new ResEnterStage();
    }

    [Route("/v1/stage/clearstage")]
    [HttpPost]
    public ActionResult<ResClearStage> ClearStage([FromBodyProtobuf] ReqClearStage req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        Console.WriteLine($"Stage " + req.StageId + " completed, result is " + req.BattleResult);

        ResClearStage response = new();

        // TODO: check if user has already cleared this stage
        if (req.BattleResult == 1)
        {
            response = CompleteStage(user, req.StageId);
        }

        db.SaveChanges();

        return response;
    }


    private ResClearStage CompleteStage(GameUser user, int StageId, bool forceCompleteScenarios = false)
    {
        ResClearStage response = new()
        {
            OutpostTimeRewardBuff = new()
        };
        CampaignStageRecord clearedStage = GameData.Instance.GetStageData(StageId) ?? throw new Exception("cleared stage cannot be null");

        string stageMapId = GameData.Instance.GetMapIdFromChapter(clearedStage.ChapterId, clearedStage.ChapterMod);

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == stageMapId);

        if (field == null)
        {
            field = new FieldInfo()
            {
                MapName = stageMapId
            };
            user.FieldInfo.Add(field);
            db.SaveChanges();
        }

        DoQuestSpecificUserOperations(user, StageId);
        RewardRecord? rewardData = GameData.Instance.GetRewardTableEntry(clearedStage.RewardId);

        if (forceCompleteScenarios)
        {
            if (!user.ViewedScenarios.Contains(clearedStage.EnterScenario) && !string.IsNullOrEmpty(clearedStage.EnterScenario) && !string.IsNullOrWhiteSpace(clearedStage.EnterScenario))
            {
                user.ViewedScenarios.Add(clearedStage.EnterScenario);
            }
            if (!user.ViewedScenarios.Contains(clearedStage.ExitScenario) && !string.IsNullOrEmpty(clearedStage.ExitScenario) && !string.IsNullOrWhiteSpace(clearedStage.ExitScenario))
            {
                user.ViewedScenarios.Add(clearedStage.ExitScenario);
            }
        }

        int oldLevel = user.UserLevel;
        int oldOutpostLevel = user.OutpostBattleLevel;

        if (rewardData != null)
            response.StageClearReward = InventoryService.AddReward(user, rewardData);
        else
            Console.WriteLine("rewardId is null for stage " + StageId);

        response.ScenarioReward = new NetRewardData() { PassPoint = new() };

        response.OutpostBattleLevelReward = new NetRewardData() { PassPoint = new() };

        // Check if user level changed, if so return the reward
        if (user.UserLevel != oldLevel)
        {
            response.UserLevelUpReward = new NetRewardData();
            response.UserLevelUpReward.Currency.Add(new NetCurrencyData()
            {
                Type = (int)CurrencyType.FreeCash,
                Value = 30 * (user.UserLevel - oldLevel),
                FinalValue = InventoryService.GetCurrencyAmount(user, CurrencyType.FreeCash)
            });
        }
        // Check if outpost level changed, if so return the reward
        if (user.OutpostBattleLevel != oldOutpostLevel)
        {
            response.OutpostBattleLevelReward = new NetRewardData();
            response.OutpostBattleLevelReward.Currency.Add(new NetCurrencyData()
            {
                Type = (int)CurrencyType.FreeCash,
                Value = 100 * (user.OutpostBattleLevel - oldOutpostLevel),
                FinalValue = InventoryService.GetCurrencyAmount(user, CurrencyType.FreeCash)
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
            if (user.MainQuestData.Where(x => x.QuestId == 21).Any())
            {
                user.OutpostBattleLevelExp++;
                if (user.OutpostBattleLevelExp >= 5)
                {
                    user.OutpostBattleLevelExp = 0;
                    user.OutpostBattleLevel++;
                    response.OutpostBattle = new NetOutpostBattleLevel() { IsLevelUp = true, Exp = 0, Level = user.OutpostBattleLevel };
                    InventoryService.AddCurrency(user, CurrencyType.FreeCash, 100); // todo is reward the same for all level upgrades
                }
                else
                {
                    response.OutpostBattle = new NetOutpostBattleLevel() { IsLevelUp = false, Exp = user.OutpostBattleLevelExp, Level = user.OutpostBattleLevel };
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

        field.CompletedStages.Add(StageId);

        bool hasGroupClear = user.Triggers.Any(t => t.UserId == user.ID && t.Type == Trigger.CampaignGroupClear && t.ConditionId == clearedStage.GroupId);
        if (!hasGroupClear)
        {
            bool groupCleared = GameData.Instance.StageDataRecords.Values
                .Where(s => s.GroupId == clearedStage.GroupId)
                .All(s => user.FieldInfo.Any(f => f.CompletedStages.Contains(s.Id)));

            if (groupCleared)
                user.AddTrigger(Trigger.CampaignGroupClear, 1, clearedStage.GroupId);
        }

        db.SaveChanges();
        return response;
    }

    private void DoQuestSpecificUserOperations(GameUser user, int clearedStageId)
    {
        MainQuestRecord? quest = GameData.Instance.GetMainQuestForStageClearCondition(clearedStageId);

        user.AddTrigger(Trigger.CampaignClear, 1, clearedStageId);
        if (quest != null)
        {
            if (!user.MainQuestData.Where(x => x.QuestId == quest.Id).Any())
            {
                user.MainQuestData.Add(new QuestProgress()
                {
                    GameUser = user,
                    QuestId = quest.Id,
                    IsRewardRecieved = false
                });
            }

            user.AddTrigger(Trigger.MainQuestClear, 1, quest.Id);
        }

        // TODO: Is this the right place to add default characters?
        // Stage 1-4 BOSS
        if (clearedStageId == 6001004)
        {
            // TID: Character ID
            // CSN: Character Serial Number

            user.Characters.Add(new CharacterModel() { Tid = 201001, NameCode = 3001, RareType = OriginalRareType.SR });
            user.Characters.Add(new CharacterModel() { Tid = 330501, NameCode = 1018, RareType = OriginalRareType.R });
            user.Characters.Add(new CharacterModel() { Tid = 130201, NameCode = 1015, RareType = OriginalRareType.R });
            user.Characters.Add(new CharacterModel() { Tid = 230101, NameCode = 1014, RareType = OriginalRareType.R });
            user.Characters.Add(new CharacterModel() { Tid = 301201, NameCode = 3005, RareType = OriginalRareType.SR });

            user.AddTrigger(Trigger.ObtainCharacter, 1, 3001);
            user.AddTrigger(Trigger.ObtainCharacter, 1, 1018);
            user.AddTrigger(Trigger.ObtainCharacter, 1, 1015);
            user.AddTrigger(Trigger.ObtainCharacter, 1, 1014);
            user.AddTrigger(Trigger.ObtainCharacter, 1, 3005);
            user.AddTrigger(Trigger.ObtainCharacterNew, 1);

            // force character IDs to be generated
            db.SaveChanges();
            
            TeamModel team = new()
            {
                TeamNumber = 1,
                LastContentsTeamNumber = 1,
                TeamType = 1,
                User = user,
            };
            user.RepresentationTeamDataNew = new long[5];

            for (int i = 0; i < 5; i++)
            {
                CharacterModel character = user.Characters.ElementAt(i);
                team.SlotIds[i] = character.Csn;

                // set profile characters
                user.RepresentationTeamDataNew[i] = character.Csn;
            }
            user.Teams.Add(team);
        }
    }
}
