using EpinelPS.Database;
using EpinelPS.LobbyServer;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Controllers
{
    [Route("adminapi")]
    [ApiController]
    public class AdminApiController : ControllerBase
    {
        public static Dictionary<string, User> AdminAuthTokens = new();
        private static MD5 md5 = MD5.Create();

        [HttpPost]
        [Route("login")]
        public LoginApiResponse Login([FromBody] LoginApiBody b)
        {
            User? user = null;
            bool nullusernames = false;
            if (b.Username != null && b.Password != null)
            {
                var passwordHash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(b.Password))).ToLower();
                foreach (var item in JsonDb.Instance.Users)
                {
                    if (item.Username == b.Username)
                    {
                        if (item.Password.ToLower() == passwordHash)
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
                if (nullusernames)
                {
                    return new LoginApiResponse() { Message = "Please enter a username and password" };
                }
                else
                {
                    return new LoginApiResponse() { Message = "Username or password is incorrect" };
                }
            }
            else
            {
                if (user.IsAdmin)
                {
                    var tok = CreateAuthToken(user);
                    HttpContext.Response.Cookies.Append("token", tok);
                    return new LoginApiResponse() { OK = true, Token = tok };
                }
                else
                {
                    return new LoginApiResponse() { Message = "User is not an administrator." };
                }
            }

        }

        private static string CreateAuthToken(User user)
        {
            var tok = RandomString(128);
            AdminAuthTokens.Add(tok, user);
            return tok;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        public class LoginApiBody
        {
            [Required]
            public string Username { get; set; } = "";
            [Required]
            public string Password { get; set; } = "";
        }
        public class LoginApiResponse
        {
            public string Message { get; set; } = "";
            public bool OK { get; set; }
            public string Token { get; set; } = "";
        }
    }
}