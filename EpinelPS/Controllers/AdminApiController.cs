using EpinelPS.Controllers.AdminPanel;
using EpinelPS.Database;
using EpinelPS.Models.Admin;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Controllers;

[Route("adminapi")]
[ApiController]
public class AdminApiController : ControllerBase
{
    private static readonly MD5 md5 = MD5.Create();

    [HttpPost]
    [Route("login")]
    public LoginApiResponse Login([FromBody] LoginApiBody b)
    {
        User? user = null;
        bool nullusernames = false;
        if (b.Username != null && b.Password != null)
        {
            string passwordHash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(b.Password))).ToLower();
            foreach (User item in JsonDb.Instance.Users)
            {
                if (item.Username == b.Username && item.Password != null)
                {
                    if (item.Password.Equals(passwordHash, StringComparison.OrdinalIgnoreCase))
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
                string tok = CreateAuthToken(user);
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

        if (JsonDb.Instance.Users.Where(x => x.Username == req.Email).Count() != 0)
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

        JsonDb.Instance.Users.Add(new User()
        {
            Username = req.Email,
            Password = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(req.Password))).ToLower(),
            RegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ID = uid,
            PlayerName = "Player_" + Rng.RandomString(8),
            IsAdmin = JsonDb.Instance.Users.Count == 0
        });

        JsonDb.Save();

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
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == ulong.Parse(req.p1));
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
                        return new RunCmdResponse() { error = "参数不足" };
                    if (!ulong.TryParse(parts[0], out ulong userId))
                        return new RunCmdResponse() { error = "无效的用户ID" };
                    User? user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
                    if (user == null)
                        return new RunCmdResponse() { error = "用户不存在" };
                    if (!int.TryParse(parts[1], out int senderId))
                        return new RunCmdResponse() { error = "无效的发件人ID" };
                    string title = parts[2];
                    string content = parts[3];
                    if (!int.TryParse(parts[4], out int validDays))
                        return new RunCmdResponse() { error = "无效的有效天数" };
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

    private static string CreateAuthToken(User user)
    {
        string tok = RandomString(128);
        // 只保留一个token
        JsonDb.Instance.AdminAuthTokens.Clear();
        JsonDb.Instance.AdminAuthTokens.Add(tok, user.ID);
        JsonDb.Save();
        return tok;
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string([.. Enumerable.Repeat(chars, length).Select(static s => s[new Random().Next(s.Length)])]);
    }
}