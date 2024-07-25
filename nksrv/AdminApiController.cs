using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using nksrv.Database;
using System.Security.Cryptography;
using System.Text;

namespace nksrv
{
    public class AdminApiController : WebApiController
    {
        public static Dictionary<string, User> AdminAuthTokens = new();
        private static MD5 md5 = MD5.Create();
        [Route(HttpVerbs.Any, "/login")]
        public async Task Login()
        {
            var c = await HttpContext.GetRequestFormDataAsync();
            var username = c["username"];
            var password = c["password"];

            if (HttpContext.Request.HttpMethod != "POST")
            {
                await HttpContext.SendStringAsync(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/www/admin/index.html").Replace("<errormsg/>", ""), "text/html", Encoding.Unicode);
                return;
            }

            User? user = null;
            bool nullusernames = false;
            if (username != null && password != null)
            {
                var passwordHash = Convert.ToHexString(md5.ComputeHash(Encoding.ASCII.GetBytes(password))).ToLower();
                foreach (var item in JsonDb.Instance.Users)
                {
                    if (item.Username == username)
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
                if (nullusernames == false)
                {
                    await HttpContext.SendStringAsync((string)File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/www/admin/index.html").Replace("<errormsg/>", "Incorrect username or password"), "text/html", Encoding.Unicode);
                    return;
                }
                else
                {
                    await HttpContext.SendStringAsync((string)File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/www/admin/index.html").Replace("<errormsg/>", "Please enter a username or password"), "text/html", Encoding.Unicode);
                    return;
                }
            }
            else
            {
                if (user.IsAdmin)
                {
                    Response.Headers.Add("Set-Cookie", "token=" + CreateAuthToken(user) + ";path=/");
                    HttpContext.Redirect("/admin/", 301);
                }
                else
                {
                    await HttpContext.SendStringAsync((string)File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/www/admin/index.html").Replace("<errormsg/>", "User does not have admin panel access."), "text/html", Encoding.Unicode);
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
    }
}
