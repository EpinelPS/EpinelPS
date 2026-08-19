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
/// Controller for unlocking
/// </summary>
[ApiController]
public class ContentsOpen(IUserService db) : Controller
{
    [Route("/v1/contentsopen/get/unlock")]
    [HttpPost]
    public ActionResult<ResGetContentsOpenUnlockInfo> GetUnlocked([FromBodyProtobuf] ReqGetContentsOpenUnlockInfo req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResGetContentsOpenUnlockInfo response = new();

        if (user.ContentsOpenUnlocked.Count == 0)
        {
            // These Always returned as true by official server
            // Fixes "Recruitment unlocked" during chapter 0
            // TODO: Don't hardcode this, maybe its in GameData
            user.ContentsOpenUnlocked.Add(3, new(true, true));
            user.ContentsOpenUnlocked.Add(4, new(true, true));
            user.ContentsOpenUnlocked.Add(6, new(true, true));
            user.ContentsOpenUnlocked.Add(15, new(true, true));
            user.ContentsOpenUnlocked.Add(16, new(true, true));
            user.ContentsOpenUnlocked.Add(18, new(true, true));
            user.ContentsOpenUnlocked.Add(19, new(true, true));
            user.ContentsOpenUnlocked.Add(2, new(true, true)); // Shop
            JsonDb.Save();
        }

        foreach (KeyValuePair<int, UnlockData> item in user.ContentsOpenUnlocked.OrderBy(x => x.Key))
        {
            response.ContentsOpenUnlockInfoList.Add(new NetContentsOpenUnlockInfo()
            {
                ContentsOpenTableId = item.Key,
                IsUnlockButtonPlayed = item.Value.ButtonAnimationPlayed,
                IsUnlockPopupPlayed = item.Value.PopupAnimationPlayed,
            });
        }

        return response;
    }

    [Route("/v1/contentsopen/set/unlock/button")]
    [HttpPost]
    public ActionResult<ResSetContentsOpenUnlockButtonPlay> SetButtonUnlocked([FromBodyProtobuf] ReqSetContentsOpenUnlockButtonPlay req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResSetContentsOpenUnlockButtonPlay response = new();

        // Unlock button animation completed
        foreach (int item in req.ContentsOpenTableIds)
        {
            if (user.ContentsOpenUnlocked.TryGetValue(item, out UnlockData? data))
            {
                data.ButtonAnimationPlayed = true;
            }
            else
            {
                user.ContentsOpenUnlocked.Add(item, new UnlockData()
                {
                    ButtonAnimationPlayed = true
                });
            }
        }

        JsonDb.Save();
        return response;
    }

    [Route("/v1/contentsopen/set/unlock/popup")]
    [HttpPost]
    public ActionResult<ResSetContentsOpenUnlockPopupPlay> SetButtonUnlocked([FromBodyProtobuf] ReqSetContentsOpenUnlockPopupPlay req)
    {
        User? user = db.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        ResSetContentsOpenUnlockPopupPlay response = new();

        foreach (int item in req.ContentsOpenTableIds)
        {
            if (user.ContentsOpenUnlocked.TryGetValue(item, out UnlockData? data))
            {
                data.PopupAnimationPlayed = true;
            }
            else
            {
                user.ContentsOpenUnlocked.Add(item, new UnlockData()
                {
                    PopupAnimationPlayed = true
                });
            }
        }

        JsonDb.Save();

        return response;
    }
}
