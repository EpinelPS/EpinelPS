using EpinelPS.Controllers.AdminPanel;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models.Admin;
using EpinelPS.Utils;
using EpinelPS.Commands.Core;
using EpinelPS.Commands.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Paseto;
using Paseto.Builder;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Controllers;

[Route("adminapi")]
[ApiController]
public class AdminApiController(GameContext DbContext) : ControllerBase
{
    private readonly GameContext dbContext = DbContext;
    private readonly CommandRegistry registry = new();
    private static readonly MD5 md5 = MD5.Create();

    [HttpPost]
    [Route("login")]
    public LoginApiResponse Login([FromBody] LoginApiBody b)
    {
        SdkUser? user = null;
        bool nullusernames = false;
        if (b.Username != null && b.Password != null)
        {
            string passwordHash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(b.Password))).ToLower();
            foreach (var item in dbContext.SdkUsers)
            {
                if (item.Email == b.Username && item.PasswordHash != null)
                {
                    if (item.PasswordHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase))
                    {
                        user = item;
                    }
                }
            }
        }
        else
        {
            nullusernames = true;
        }

        if (user == null)
        {
            return nullusernames
                ? new LoginApiResponse() { Message = "Please enter a username and password" }
                : new LoginApiResponse() { Message = "Username or password is incorrect" };
        }
        else
        {
            if (user.IsAdmin)
            {
                string tok = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                    .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                    .AddClaim("userId", user.ID)
                    .IssuedAt(DateTime.UtcNow)
                    .Expiration(DateTime.UtcNow.AddDays(2))
                    .Encode();
                HttpContext.Response.Cookies.Append("token", tok);
                return new LoginApiResponse() { OK = true, Token = tok };
            }
            else
            {
                return new LoginApiResponse() { Message = "User is not an administrator." };
            }
        }
    }

    [HttpPost("RegisterAccount")]
    public RunCmdResponse RegisterAccount([FromBody] RegisterAccountReg req)
    {
        if (!AdminController.CheckAuth(HttpContext) && JsonDb.Instance.Users.Count != 0) return new RunCmdResponse() { error = "bad token" };

        if (dbContext.SdkUsers.Where(x => x.Email == req.Email).Count() != 0)
        {
            return new RunCmdResponse() { error = $"Email {req.Email} already exists" };
        }

        ulong uid = (ulong)new Random().Next(1, int.MaxValue);

        // Check if we havent generated a UID that exists
        foreach (User item in JsonDb.Instance.Users)
        {
            if (item.ID == uid)
            {
                uid -= (ulong)new Random().Next(1, 1221);
            }
        }

        bool admin = JsonDb.Instance.Users.Count == 0;

        JsonDb.Instance.Users.Add(new User()
        {
            ID = uid
        });

        dbContext.SdkUsers.Add(new SdkUser()
        {
            ID = uid,
            Email = req.Email,
            PasswordHash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(req.Password))).ToLower(),
            RegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsAdmin = admin,
            PlayerName = "Player_" + Rng.RandomString(8),
        });

        dbContext.Users.Add(new GameUser()
        {
            ID = uid // todo remove later
        });

        JsonDb.Save();
        dbContext.SaveChanges();

        return new RunCmdResponse() { ok = true };
    }

    [HttpPost("RunCmd")]
    public async Task<RunCmdResponse> RunCmd([FromBody] RunCmdRequest req)
    {
        if (!AdminController.CheckAuth(HttpContext)) return new RunCmdResponse() { error = "bad token" };

        // --- SendMail uses pipe-delimited format (title/content may contain spaces) ---
        if (req.cmdName.Equals("send-mail", StringComparison.OrdinalIgnoreCase))
        {
            if (!ulong.TryParse(req.p1, out ulong mailUserId))
                return new RunCmdResponse() { error = "Invalid user ID" };
            User? mailUser = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == mailUserId);
            if (mailUser == null)
                return new RunCmdResponse() { error = "User not found" };

            // p2 is pipe-delimited: senderId|title|content|validDays|attachments
            string[] parts = (req.p2 ?? "").Split('|');

            var mailArgs = new[] { parts[0], parts[1], parts[2], parts[3] };
            if (parts.Length > 4 && !string.IsNullOrEmpty(parts[4]))
                mailArgs = [.. mailArgs, parts[4]];

            var mailCtx = new CliContext { SelectedUser = mailUser };
            var mailHandler = registry.CreateHandler("send-mail", mailCtx);
            if (mailHandler == null)
                return new RunCmdResponse() { error = "send-mail command not available via API" };

            var mailResult = await mailHandler.ExecuteAsync(mailArgs);
            mailCtx.Save();
            return mailResult.ToRunCmdResponse();
        }

        // --- Generic dispatch ---
        // Try to resolve user context from p1 (non-fatal for user-less commands like reload-db, update-server)
        User? user = null;
        if (ulong.TryParse(req.p1, out ulong userId))
        {
            user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
        }

        var ctx = new CliContext { SelectedUser = user };

        // Look up handler
        var handler = registry.CreateHandler(req.cmdName, ctx);
        if (handler == null)
        {
            // Check if the command exists at all (to give a better error message)
            var existing = registry.FindHandler(req.cmdName);
            if (existing != null)
                return new RunCmdResponse() { error = $"Command '{req.cmdName}' is not available via API" };

            return new RunCmdResponse() { error = $"Unknown command: {req.cmdName}" };
        }

        var args = req.ToArgs();
        var result = await handler.ExecuteAsync(args);

        ctx.Save();

        return result.ToRunCmdResponse();
    }
}
