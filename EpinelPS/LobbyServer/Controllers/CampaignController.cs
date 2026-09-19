using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Services;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for game startup and asset information retrival
/// </summary>
[ApiController]
public class CampaignController(IUserService UserController, GameContext db, IInventoryService Inventory) : Controller
{
    [Route("/v1/shutdownflags/campaignpackage/getall")]
    [HttpPost]
    public ActionResult<ResCampaignPackageGetAllShutdownFlags> GetUnlocked([FromBodyProtobuf] ReqCampaignPackageGetAllShutdownFlags req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResCampaignPackageGetAllShutdownFlags();
    }

    [Route("/v1/campaign/getfield")]
    [HttpPost]
    public ActionResult<ResGetCampaignFieldData> GetCampaignField([FromBodyProtobuf] ReqGetCampaignFieldData req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetCampaignFieldData response = new()
        {
            Field = StageController.CreateFieldInfo(user, req.MapId, out bool bossEntered),

            // todo save this data
            Team = new NetUserTeamData() { LastContentsTeamNumber = 1, Type = 1 }
        };
        if (user.LastNormalStageCleared >= 6000003)
        {
            NetTeamData team = new() { TeamNumber = 1 };
            team.Slots.Add(new NetTeamSlot() { Slot = 1, Value = 47263455 });
            team.Slots.Add(new NetTeamSlot() { Slot = 2, Value = 47263456 });
            team.Slots.Add(new NetTeamSlot() { Slot = 3, Value = 47263457 });
            team.Slots.Add(new NetTeamSlot() { Slot = 4, Value = 47263458 });
            team.Slots.Add(new NetTeamSlot() { Slot = 5, Value = 47263459 });
            response.Team.Teams.Add(team);

            response.TeamPositions.Add(new NetCampaignTeamPosition() { TeamNumber = 1, Type = 1, Position = new NetVector3() { } });
        }

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == req.MapId);

        if (field == null)
        {
            field = new FieldInfo
            {
                MapName = req.MapId
            };
            user.FieldInfo.Add(field);
        }

        response.Json = field.PositionJson;
        return response;
    }

    [Route("/v1/campaign/savefield")]
    [HttpPost]
    public async Task<ActionResult<ResSaveCampaignField>> SaveField([FromBodyProtobuf] ReqSaveCampaignField req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResSaveCampaignField response = new();

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == req.MapId);

        if (field == null)
        {
            field = new FieldInfo
            {
                MapName = req.MapId
            };
            user.FieldInfo.Add(field);
        }

        field.PositionJson = req.Json;

        await db.SaveChangesAsync();
        return response;
    }

    [Route("/v1/campaign/savefieldobject")]
    [HttpPost]
    public ActionResult<ResSaveCampaignFieldObject> SaveObject([FromBodyProtobuf] ReqSaveCampaignFieldObject req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResSaveCampaignFieldObject response = new();

        Logging.WriteLine($"save {req.MapId} with {req.FieldObject.PositionId}", LogType.Debug);

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == req.MapId);

        if (field == null)
        {
            field = new FieldInfo
            {
                MapName = req.MapId
            };
            user.FieldInfo.Add(field);
        }

        field.CompletedObjects.Add(new CompletedFieldObject() { PositionId = req.FieldObject.PositionId, Json = req.FieldObject.Json, Type = req.FieldObject.Type, User = user });
        db.SaveChanges();

        return response;
    }

    [Route("/v1/campaign/getfieldobjectitemsnum")]
    [HttpPost]
    public ActionResult<ResGetCampaignFieldObjectItemsNum> GetFieldObjectCountTotal([FromBodyProtobuf] ReqGetCampaignFieldObjectItemsNum req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetCampaignFieldObjectItemsNum response = new();

        foreach (var map in user.FieldInfo)
        {
            response.FieldObjectItemsNum.Add(new NetCampaignFieldObjectItemsNum()
            {
                MapId = map.MapName,
                Count = map.CompletedObjects.Where(x => x.Type == 1).Count()
            });
        }

        return response;
    }

    [Route("/v1/campaign/obtain/item")]
    [HttpPost]
    public ActionResult<ResObtainCampaignItem> ObtainItem([FromBodyProtobuf] ReqObtainCampaignItem req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResObtainCampaignItem response = new();

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == req.MapId);

        if (field == null)
        {
            field = new FieldInfo
            {
                MapName = req.MapId
            };
            user.FieldInfo.Add(field);
        }


        foreach (var item in field.CompletedObjects)
        {
            if (item.PositionId == req.FieldObject.PositionId)
            {
                Logging.WriteLine("attempted to collect campaign field object twice!", LogType.WarningAntiCheat);
                return Problem(type: NetUtils.AnticheatError);
            }
        }

        // Register and return reward
        var map = GameData.Instance.MapData[req.MapId];

        var position = map.ItemSpawner.Where(x => x.PositionId == req.FieldObject.PositionId).FirstOrDefault() ?? throw new Exception("bad position Id");

        FieldItemRecord positionReward = GameData.Instance.FieldItems[position.ItemId];
        RewardRecord reward = GameData.Instance.GetRewardTableEntry(positionReward.TypeValue) ?? throw new Exception("failed to get reward");
        response.Reward = Inventory.AddReward(user, reward);

        // HIde it from the field
        field.CompletedObjects.Add(new CompletedFieldObject() { PositionId = req.FieldObject.PositionId, Type = req.FieldObject.Type, ActionAt = DateTime.UtcNow,
        Json = req.FieldObject.Json, UserId = user.ID });

        db.SaveChanges();
        return response;
    }

    [Route("/v1/user/getcontentsdata")]
    [HttpPost]
    public ActionResult<ResGetContentsOpenData> GetContentsData([FromBodyProtobuf] ReqGetContentsOpenData req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        var response = new ResGetContentsOpenData();

         List<int> stages = [];

        foreach (var item in GameData.Instance.ContentsOpenTable)
        {
            foreach (var condition in item.Value.OpenCondition)
            {
                if (condition.OpenConditionType == ContentsOpenCondition.StageClear && !stages.Contains(condition.OpenConditionValue) && user.FieldInfo.Any(x => x.CompletedStages.Contains(condition.OpenConditionValue)))
                {
                    stages.Add(condition.OpenConditionValue);
                }
            }
        }

        // these stages are not present in contentsopentable but are required to show mission UI and burst sidebar UI in battle view
        List<int> specialStages = [6000001, 6000003];

        foreach (var item in specialStages)
        {
            if (!stages.Contains(item) && user.FieldInfo.Any(x => x.CompletedStages.Contains(item))) stages.Add(item);
        }

        response.ClearStageList.AddRange(stages);
        //response.MaxGachaCount = user.GetGachaTotalCount();
        //response.MaxGachaPremiumCount = user.GetGachaCountForType(GachaPremiumType.GachaPremium);
        // todo tutorial playcount of gacha
        //response.TutorialGachaPlayCount = user.GetGachaCountForType(GachaPremiumType.GachaTutorial);      

        // ClearSimRoomChapterList: 已通关的章节列表，用于显示超频选项 SimRoomOC
      //  response.ClearSimRoomChapterList.AddRange(GetClearSimRoomChapterList(user));
        return response;
    }

    [Route("/v1/mission/getrewarded/jukebox")]
    [HttpPost]
    public ActionResult<ResGetJukeboxRewardedData> GetJukeboxRewards([FromBodyProtobuf] ReqGetJukeboxRewardedData req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetJukeboxRewardedData();
    }

    [Route("/v1/mission/getrewarded/all")]
    [HttpPost]
    public ActionResult<ResGetAchievementRewardedData> GetAchievementRewardedData([FromBodyProtobuf] ReqGetAchievementRewardedData req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetAchievementRewardedData();
    }

    [Route("/v1/mission/getrewarded/weekly")]
    [HttpPost]
    public ActionResult<ResGetWeeklyRewardedData> GetRewardedWeekly([FromBodyProtobuf] ReqGetWeeklyRewardedData req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetWeeklyRewardedData();
    }

    [Route("/v1/mission/getrewarded/daily")]
    [HttpPost]
    public ActionResult<ResGetDailyRewardedData> GetRewardedWeekly([FromBodyProtobuf] ReqGetDailyRewardedData req)
    {
        GameUser? user = UserController.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetDailyRewardedData();
    }
}
