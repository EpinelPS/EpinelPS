using EpinelPS.Controllers.AdminPanel;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Models.Admin;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Controllers
{
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
}