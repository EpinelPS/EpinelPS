using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Interfaces;
using EpinelPS.Utils;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Paseto;
using Paseto.Builder;
using System.Text.Json;

namespace EpinelPS.LobbyServer.Controllers;

/// <summary>
/// Controller for level infinite authentication
/// </summary>
[ApiController]
public class AuthController(IUserService UserService, GameContext db) : Controller
{
    /// <summary>
    /// Deletes auth token
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Route("/v1/logout")]
    [HttpPost]
    public ActionResult<ResLogout> Logout([FromBodyProtobuf] ReqLogout req)
    {
        User? user = UserService.GetUser();
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        // TODO delete auth token
        return new ResLogout();
    }

    /// <summary>
    /// Deletes auth token
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Route("/v1/auth/intl")]
    [HttpPost]
    public ActionResult<ResAuth> DoIntlAuth([FromBodyProtobuf] ReqAuthIntl req)
    {
        var response = new ResAuth();
        var sdkUser = NetUtils.GetUser(req.Token, HttpContext).Item1;
        if (sdkUser == null) return Problem(type: NetUtils.InvalidSessionErrorType);

        var UserId = sdkUser.ID;
        User? user = JsonDb.GetUser(UserId);
        if (user == null)
        {
            return Problem(type: NetUtils.InvalidSessionErrorType);
        }

        if (user.IsBanned && user.BanEnd < DateTime.UtcNow)
        {
            user.IsBanned = false;
            user.BanId = 0;
            user.BanStart = DateTime.MinValue;
            user.BanEnd = DateTime.MinValue;
            JsonDb.Save();
        }

        if (user.IsBanned)
        {
            response.BanInfo = new NetBanInfo() { BanId = user.BanId, Description = "The server admin is sad today because the hinge on his HP laptop broke which happened to be an HP Elitebook 8470p, and the RAM controller exploded and then fixed itself, please contact him", StartAt = Timestamp.FromDateTime(DateTime.SpecifyKind(user.BanStart, DateTimeKind.Utc)), EndAt = Timestamp.FromDateTime(DateTime.SpecifyKind(user.BanEnd, DateTimeKind.Utc)) };
        }
        else
        {
            response.AuthSuccess = new NetAuthSuccess() { AuthToken = req.Token, CentauriZoneId = "84", FirstAuth = false, PurchaseRestriction = new NetUserPurchaseRestriction() { PurchaseRestriction = PurchaseRestriction.Child, UpdatedAt = 638546758794611090 } };
        }


        return response;
    }

    /// <summary>
    /// Deletes auth token
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Route("/v1/auth/enterserver")]
    [HttpPost]
    public ActionResult<ResEnterServer> EnterServer([FromBodyProtobuf] ReqEnterServer req)
    {
        var sdkUser = NetUtils.GetUser(req.AuthToken, HttpContext).Item1;
        if (sdkUser == null) return Problem(type: NetUtils.InvalidSessionErrorType);
        var UserId = sdkUser.ID;

        User? user = JsonDb.GetUser(UserId);
        if (user == null) return Problem(type: NetUtils.InvalidSessionErrorType);


        GameClientInfo rsp = LobbyHandler.GenGameClientTok(req.ClientPublicKey, UserId);

        string token = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                           .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                           .AddClaim("userId", UserId)
                           .IssuedAt(DateTime.UtcNow)
                           .Expiration(DateTime.UtcNow.AddDays(2))
                           .Encode();

        string encryptionToken = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                           .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                           .AddClaim("data", JsonSerializer.Serialize(rsp))
                           .IssuedAt(DateTime.UtcNow)
                           .Expiration(DateTime.UtcNow.AddDays(2))
                           .Encode();


        ResEnterServer response = new()
        {
            GameClientToken = token,
            FeatureDataInfo = new NetFeatureDataInfo() { }, // TODO
            Identifier = new NetLegacyUserIdentifier() { Server = 1000, Usn = (long)user.ID },
            ShouldRestartAfter = Duration.FromTimeSpan(TimeSpan.FromSeconds(86400)),

            EncryptionToken = ByteString.CopyFromUtf8(encryptionToken)
        };

        user.ResetDataIfNeeded();

        return response;
    }
}
