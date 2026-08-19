using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Services;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using EpinelPS.LobbyServer.Stage;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for game startup and asset information retrival
/// </summary>
[ApiController]
public class CampaignController(IUserService db) : Controller
{
    [Route("/v1/shutdownflags/campaignpackage/getall")]
    [HttpPost]
    public ActionResult<ResCampaignPackageGetAllShutdownFlags> GetUnlocked([FromBodyProtobuf] ReqCampaignPackageGetAllShutdownFlags req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResCampaignPackageGetAllShutdownFlags();
    }

    [Route("/v1/campaign/getfield")]
    [HttpPost]
    public ActionResult<ResGetCampaignFieldData> GetCampaignField([FromBodyProtobuf] ReqGetCampaignFieldData req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetCampaignFieldData response = new()
        {
            Field = GetStage.CreateFieldInfo(user, req.MapId, out bool bossEntered),

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

        string resultingJson;
        if (!user.MapJson.TryGetValue(req.MapId, out string? value))
        {
            resultingJson = "";
            user.MapJson.Add(req.MapId, resultingJson);
        }
        else
        {
            resultingJson = value;
        }

        response.Json = resultingJson;
        return response;
    }

    [Route("/v1/campaign/savefield")]
    [HttpPost]
    public ActionResult<ResSaveCampaignField> SaveField([FromBodyProtobuf] ReqSaveCampaignField req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResSaveCampaignField response = new();

        if (!user.MapJson.ContainsKey(req.MapId))
        {
            user.MapJson.Add(req.MapId, req.Json);
        }
        else
        {
            user.MapJson[req.MapId] = req.Json;
        }
        return response;
    }

    [Route("/v1/campaign/savefieldobject")]
    [HttpPost]
    public ActionResult<ResSaveCampaignFieldObject> SaveObject([FromBodyProtobuf] ReqSaveCampaignFieldObject req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResSaveCampaignFieldObject response = new();

        Logging.WriteLine($"save {req.MapId} with {req.FieldObject.PositionId}", LogType.Debug);

        FieldInfoNew field = user.FieldInfoNew[req.MapId];

        field.CompletedObjects.Add(new NetFieldObject() { PositionId = req.FieldObject.PositionId, Json = req.FieldObject.Json, Type = req.FieldObject.Type });
        JsonDb.Save();

        return response;
    }

    [Route("/v1/campaign/getfieldobjectitemsnum")]
    [HttpPost]
    public ActionResult<ResGetCampaignFieldObjectItemsNum> GetFieldObjectCountTotal([FromBodyProtobuf] ReqGetCampaignFieldObjectItemsNum req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetCampaignFieldObjectItemsNum response = new();

        foreach (KeyValuePair<string, FieldInfoNew> map in user.FieldInfoNew)
        {
            response.FieldObjectItemsNum.Add(new NetCampaignFieldObjectItemsNum()
            {
                MapId = map.Key,
                Count = map.Value.CompletedObjects.Where(x => x.Type == 1).Count()
            });
        }

        return response;
    }

    [Route("/v1/campaign/obtain/item")]
    [HttpPost]
    public ActionResult<ResObtainCampaignItem> ObtainItem([FromBodyProtobuf] ReqObtainCampaignItem req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResObtainCampaignItem response = new();

        if (!user.FieldInfoNew.TryGetValue(req.MapId, out FieldInfoNew? field))
        {
            field = new FieldInfoNew();
            user.FieldInfoNew.Add(req.MapId, field);
        }


        foreach (NetFieldObject item in field.CompletedObjects)
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
        response.Reward = RewardUtils.RegisterRewardsForUser(user, reward);

        // HIde it from the field
        field.CompletedObjects.Add(new NetFieldObject() { PositionId = req.FieldObject.PositionId, Type = req.FieldObject.Type });

        JsonDb.Save();
        return response;
    }
}
