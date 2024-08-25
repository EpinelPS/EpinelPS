using EpinelPS.Database;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EpinelPS.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private const string BadAuthToken = "{\"msg\":\"the account does not exists!\",\"ret\":2001,\"seq\":\"123" + "\"}";

        [HttpPost]
        [Route("login")]
        public string Login(string seq, [FromBody] LoginEndpoint2Req req)
        {
            foreach (var item in JsonDb.Instance.Users)
            {
                if (item.Username == req.account && item.Password == req.password)
                {
                    var tok = CreateLauncherTokenForUser(item);
                    item.LastLogin = DateTime.UtcNow;
                    JsonDb.Save();

                    return "{\"expire\":" + tok.ExpirationTime + ",\"is_login\":true,\"msg\":\"Success\",\"register_time\":" + item.RegisterTime + ",\"ret\":0,\"seq\":\"" + seq + "\",\"token\":\"" + tok.Token + "\",\"uid\":\"" + item.ID + "\"}";
                }
            }

            return "{\"msg\":\"the account does not exists!\",\"ret\":2001,\"seq\":\"" + seq + "\"}";
        }


        [HttpPost]
        [Route("sendcode")]
        public string SendCode(string seq, [FromBody] SendCodeRequest req)
        {
            // Pretend that we send a code.
            return "{\"expire_time\":898,\"msg\":\"Success\",\"ret\":0,\"seq\":\"" + seq + "\"}";
        }

        [HttpPost]
        [Route("codestatus")]
        public string CodeStatus(string seq, [FromBody] SendCodeRequest req)
        {
            // Pretend that code is valid
            return "{\"expire_time\":759,\"msg\":\"Success\",\"ret\":0,\"seq\":\"" + seq + "\"}";
        }

        [HttpPost]
        [Route("getuserinfo")]
        public string GetUserInfo(string seq, [FromBody] AuthPkt2 req)
        {
            (User?, AccessToken?) res;
            if ((res = NetUtils.GetUser(req.token)).Item1 == null) return BadAuthToken;
            User user = res.Item1;
            AccessToken? tok = res.Item2;

            // Pretend that code is valid
            return "{\"account_type\":1,\"birthday\":\"1970-01\",\"email\":\"" + user.Username + "\",\"expire\":" + tok.ExpirationTime + ",\"is_receive_email\":1,\"is_receive_email_in_night\":0,\"is_receive_video\":-1,\"lang_type\":\"en\",\"msg\":\"Success\",\"nick_name\":\"\",\"phone\":\"\",\"phone_area_code\":\"\",\"privacy_policy\":\"1\",\"privacy_update_time\":1717783097,\"region\":\"724\",\"ret\":0,\"seq\":\"" + seq + "\",\"terms_of_service\":\"\",\"terms_update_time\":0,\"uid\":\"" + user.ID + "\",\"user_agreed_dt\":\"\",\"user_agreed_pp\":\"1\",\"user_agreed_tos\":\"\",\"user_name\":\"" + user.PlayerName + "\",\"username_pass_verify\":0}";
        }

        [HttpPost]
        [Route("register")]
        public string RegisterAccount(string seq, [FromBody] RegisterEPReq req)
        {
            // check if the account already exists
            foreach (var item in JsonDb.Instance.Users)
            {
                if (item.Username == req.account)
                {
                    return "{\"msg\":\"send code failed; invalid account\",\"ret\":2112,\"seq\":\"" + seq + "\"}";
                }
            }

            var uid = (ulong)new Random().Next(1, int.MaxValue);

            // Check if we havent generated a UID that exists
            foreach (var item in JsonDb.Instance.Users)
            {
                if (item.ID == uid)
                {
                    uid -= (ulong)new Random().Next(1, 1221);
                }
            }

            var user = new User() { ID = uid, Password = req.password, RegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Username = req.account, PlayerName = "Player_" + Rng.RandomString(8) };

            JsonDb.Instance.Users.Add(user);

            var tok = CreateLauncherTokenForUser(user);

            return "{\"expire\":" + tok.ExpirationTime + ",\"is_login\":false,\"msg\":\"Success\",\"register_time\":" + user.RegisterTime + ",\"ret\":0,\"seq\":\"" + seq + "\",\"token\":\"" + tok.Token + "\",\"uid\":\"" + user.ID + "\"}";
        }
        public static AccessToken CreateLauncherTokenForUser(User user)
        {
            // TODO: implement access token expiration
            AccessToken token = new() { ExpirationTime = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds() };
            token.Token = Rng.RandomString(64);
            token.UserID = user.ID;
            JsonDb.Instance.LauncherAccessTokens.Add(token);
            JsonDb.Save();

            return token;
        }
    }
}
