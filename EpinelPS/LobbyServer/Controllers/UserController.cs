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
public class UserController(IUserService userService, GameContext db) : Controller
{
    [Route("/v1/lobby/usertitle/get")]
    [HttpPost]
    public ActionResult<ResGetUserTitleList> GetUserTitle([FromBodyProtobuf] ReqGetUserTitleList req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetUserTitleList();
    }

    [Route("/v1/lobby/usertitlecounter/get")]
    [HttpPost]
    public ActionResult<ResGetUserTitleCounterList> GetUserTitle([FromBodyProtobuf] ReqGetUserTitleCounterList req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResGetUserTitleCounterList();
    }

    [Route("/v1/useronlinestatelog")]
    [HttpPost]
    public async Task<ActionResult<ResUserOnlineStateLog>> LastAction([FromBodyProtobuf] ReqUserOnlineStateLog req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        await db.Users.Where(u => u.ID == user.ID).ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastAction, DateTime.UtcNow));
        await db.SaveChangesAsync();
        return new ResUserOnlineStateLog();
    }

    [Route("/v1/wallet/refreshcharge")]
    [HttpPost]
    public async Task<ActionResult<ResRefreshChargeCurrencyData>> RefreshCharge([FromBodyProtobuf] ReqRefreshChargeCurrencyData req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO
        return new ResRefreshChargeCurrencyData();
    }

    [Route("/v1/user/getfieldtalklist")]
    [HttpPost]
    public async Task<ActionResult<ResGetFieldTalkList>> GetFieldTalkList([FromBodyProtobuf] ReqGetFieldTalkList req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        return new ResGetFieldTalkList();
    }

    [Route("/v1/User/GetScenarioList")]
    [HttpPost]
    public async Task<ActionResult<ResGetScenarioList>> GetScenarioList([FromBodyProtobuf] ReqGetScenarioList req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        var response = new ResGetScenarioList();
        foreach (var item in user.ViewedScenarios)
        {
            response.ScenarioList.Add(item);
        }
        // TODO bookmarks
        return response;
    }

    [Route("/v1/tutorial/set")]
    [HttpPost]
    public async Task<ActionResult<ResSetTutorial>> SetTutorial([FromBodyProtobuf] ReqSetTutorial req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        var cleared = GameData.Instance.GetTutorialDataById(req.LastClearedTid);

        var tutorials = user.Tutorials.Where(x => x.GroupId == cleared.GroupId);
        if (tutorials.Any())
        {
            tutorials.First().TutorialId = req.LastClearedTid;
        }
        else
        {
            user.Tutorials.Add(new ClearedTutorial()
            {
                GameUser = user,
                GroupId = cleared.GroupId,
                TutorialId = req.LastClearedTid,
                Version = cleared.VersionGroup
            });
        }
        await db.SaveChangesAsync();
        
        return new ResSetTutorial();
    }

    [Route("/v1/User/SetNickNameInTutorial")]
    [HttpPost]
    public async Task<ActionResult<ResSetNicknameInTutorial>> SetNickname([FromBodyProtobuf] ReqSetNicknameInTutorial req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        db.Users.Where(u => u.ID == user.ID).ExecuteUpdate(setters => setters.SetProperty(u => u.Nickname, req.Nickname));
        
        await db.SaveChangesAsync();
        
        return new ResSetNicknameInTutorial()
        {
            Result = SetNicknameResult.Okay,
            Nickname = req.Nickname
        };
    }

    [Route("/v1/User/Get")]
    [HttpPost]
    public async Task<ActionResult<ResGetUserData>> GetUser([FromBodyProtobuf] ReqGetUserData req)
    {
        GameUser? user = userService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetUserData response = new();
        
        TimeSpan battleTime = DateTime.UtcNow - user.BattleTime;
        long battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);


        response.User = LobbyHandler.CreateNetUserDataFromUser(user);
        response.ResetHour = JsonDb.Instance.ResetHourUtcTime;
        response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = battleTimeMs };
        response.OutpostBattleLevel = new() { Level = user.OutpostBattleLevel, Exp = user.OutpostBattleLevelExp};
        response.IsSimple = req.IsSimple;

        foreach (var item in user.Currency)
        {
            response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Type, Value = item.Amount });
        }
        response.RepresentationTeam = NetUtils.GetDisplayedTeam(user);

        response.LastClearedNormalMainStageId = user.LastNormalStageCleared;
        response.LastClearedStoryStageId = user.LastStoryStageCleared;
        response.LastClearedHardMainStageId = user.LastHardStageCleared;
        response.LastClearedMod = user.LastClearedDifficulty;

        // Restore completed tutorials. GroupID is the first 4 digits of the Table ID.
        foreach (var item in user.Tutorials)
        {
            response.User.Tutorials.Add(new NetTutorialData()
            {
                GroupId = item.GroupId,
                LastClearedTid = item.TutorialId,
                LastClearedVersion = item.Version
            });
        }

        response.CommanderRoomJukeboxBgm = new NetJukeboxBgm() { JukeboxTableId = 5/*user.CommanderMusic.TableId*/, Type = NetJukeboxBgmType.JukeboxTableId, Location = NetJukeboxLocation.CommanderRoom };
        response.LobbyJukeboxBgm = new NetJukeboxBgm() { JukeboxTableId = 2/*user.LobbyMusic.TableId*/, Type = NetJukeboxBgmType.JukeboxTableId, Location = NetJukeboxLocation.Lobby };
        return response;
    }
}
