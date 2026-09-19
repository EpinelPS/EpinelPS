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
/// Controller for user data
/// </summary>
[ApiController]
public class ScenarioController(IUserService userService, GameContext db, IInventoryService Inventory) : Controller
{
    [Route("/v1/user/scenario/exist")]
    [HttpPost]
    public ActionResult<ResExistScenario> ScenarioExists([FromBodyProtobuf] ReqExistScenario req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        var response = new ResExistScenario();

        foreach (var item in req.ScenarioGroupIds)
        {
            if (user.ViewedScenarios.Contains(item))
            {
                response.ExistGroupIds.Add(item);
            }
        }

        return response;
    }

    [Route("/v1/User/SetScenarioComplete")]
    [HttpPost]
    public ActionResult<ResSetScenarioComplete> SetScenarioComplete([FromBodyProtobuf] ReqSetScenarioComplete req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResSetScenarioComplete response = new()
        {
            Reward = new NetRewardData()
        };

        if (!user.ViewedScenarios.Contains(req.ScenarioId))
            user.ViewedScenarios.Add(req.ScenarioId);

        if (GameData.Instance.ScenarioRewards.TryGetValue(req.ScenarioId, out ScenarioRewardsRecord? record))
        {
            response.Reward = Inventory.AddReward(user, record.RewardId);
        }

        db.SaveChanges();

        return response;
    }
}
