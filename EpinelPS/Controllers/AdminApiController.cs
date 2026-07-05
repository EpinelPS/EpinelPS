using EpinelPS.Controllers.AdminPanel;
using EpinelPS.Database;
using EpinelPS.Models.Admin;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;
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

        switch (req.cmdName)
        {
            case "reloadDb":
                JsonDb.Reload();
                return RunCmdResponse.OK;
            case "completestage":
                return AdminCommands.CompleteStage(ulong.Parse(req.p1), req.p2);
            case "addallcharacters":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.AddAllCharacters(user);
                }
            case "addallmaterials":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.AddAllMaterials(user, int.Parse(req.p2));
                }
            case "SetLevel":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.SetCharacterLevel(user, int.Parse(req.p2));
                }
            case "SetSkillLevel":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.SetSkillLevel(user, int.Parse(req.p2));
                }
            case "SetCoreLevel":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.SetCoreLevel(user, int.Parse(req.p2));
                }
            case "finishalltutorials":
                {
                     GameUser? user = GameContext.Instance.Users.Find(ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.FinishAllTutorials(user);
                }
            case "AddCharacter":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };
                    return AdminCommands.AddCharacter(user, int.Parse(req.p2));
                }
            case "AddItem":
                {
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
                    if (user == null) return new RunCmdResponse() { error = "invalid user ID" };

                    string[] s = req.p2.Split("-");
                    return AdminCommands.AddItem(user, int.Parse(s[0]), int.Parse(s[1]));
                }
            case "SendMail":
                {
                    string[] parts = req.p1.Split('|');
                    if (parts.Length < 6)
                        return new RunCmdResponse() { error = "Insufficient parameters" };
                    if (!ulong.TryParse(parts[0], out ulong userId))
                        return new RunCmdResponse() { error = "Invalid user ID" };
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
                    if (user == null)
                        return new RunCmdResponse() { error = "User not found" };
                    if (!int.TryParse(parts[1], out int senderId))
                        return new RunCmdResponse() { error = "Invalid sender ID" };
                    string title = parts[2];
                    string content = parts[3];
                    if (!int.TryParse(parts[4], out int validDays))
                        return new RunCmdResponse() { error = "Invalid validity days" };
                    var attachments = new List<MailAttachment>();
                    string attachmentsParam = parts.Length > 5 ? parts[5] : "";

                    if (!string.IsNullOrEmpty(attachmentsParam))
                    {
                        foreach (var item in attachmentsParam.Split(','))
                        {
                            string[] attParts = item.Split('-');
                            if (attParts.Length != 3) continue;

                            if (int.TryParse(attParts[0], out int type) &&
                                int.TryParse(attParts[1], out int id) &&
                                int.TryParse(attParts[2], out int count))
                            {
                                attachments.Add(new MailAttachment
                                {
                                    Type = type,
                                    Id = id,
                                    Count = count
                                });
                            }
                        }
                    }

                    return AdminCommands.SendMail(user, senderId, title, content, validDays, attachments);
                }
            case "updateServer":
                {
                    return await AdminCommands.UpdateResources();
                }
        }
        return new RunCmdResponse() { error = "Not implemented" };
    }
}