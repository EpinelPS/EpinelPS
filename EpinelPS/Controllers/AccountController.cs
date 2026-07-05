using EpinelPS.Database;
using EpinelPS.Utils;
using Microsoft.AspNetCore.Mvc;
using Paseto;
using Paseto.Builder;

namespace EpinelPS.Controllers;

[Route("account")]
[ApiController]
public class AccountController(GameContext DbContext) : ControllerBase
{
    private readonly GameContext dbContext = DbContext;
    private const string BadAuthToken = "{\"msg\":\"the account does not exists!\",\"ret\":2001,\"seq\":\"123" + "\"}";

    [HttpPost]
    [Route("login")]
    public string Login(string seq, [FromBody] LoginEndpoint2Req req)
    {
        var userResults = dbContext.SdkUsers.Where(x => x.Email == req.account && x.PasswordHash == req.password);

        if (!userResults.Any())
        {
            return "{\"msg\":\"login failed; invalid account or password!\",\"ret\":2002,\"seq\":\"" + seq + "\"}";
        }

        var user = userResults.First();
        
        AccessToken tok = CreateLauncherTokenForUser(user.ID);
        user.LastLogin = DateTime.UtcNow;
        dbContext.SaveChanges();

        return "{\"expire\":" + tok.ExpirationTime + ",\"is_login\":true,\"msg\":\"Success\",\"register_time\":" + user.RegisterTime + ",\"ret\":0,\"seq\":\"" + seq + "\",\"token\":\"" + tok.Token + "\",\"uid\":\"" + user.ID + "\"}";
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
        (SdkUser?, AccessToken?) res;
        if ((res = NetUtils.GetUser(req.token, HttpContext)).Item1 == null) return BadAuthToken;
        SdkUser user = res.Item1;
        AccessToken? tok = res.Item2;

        if (tok == null)
        {
            // TODO: better error handling
            return "{}";
        }

        // Pretend that code is valid
        return "{\"account_type\":1,\"birthday\":\"1970-01\",\"email\":\"" + user.Email + "\",\"expire\":" + tok.ExpirationTime + ",\"is_receive_email\":1,\"is_receive_email_in_night\":0,\"is_receive_video\":-1,\"lang_type\":\"en\",\"msg\":\"Success\",\"nick_name\":\"\",\"phone\":\"\",\"phone_area_code\":\"\",\"privacy_policy\":\"1\",\"privacy_update_time\":1717783097,\"region\":\"724\",\"ret\":0,\"seq\":\"" + seq + "\",\"terms_of_service\":\"\",\"terms_update_time\":0,\"uid\":\"" + user.ID + "\",\"user_agreed_dt\":\"\",\"user_agreed_pp\":\"1\",\"user_agreed_tos\":\"\",\"user_name\":\"" + user.PlayerName + "\",\"username_pass_verify\":0}";
    }

    [HttpPost]
    [Route("register")]
    public string RegisterAccount(string seq, [FromBody] RegisterEPReq req)
    {
        // check if the account already exists
        if (dbContext.SdkUsers.Any(x => x.Email == req.account))
             return "{\"msg\":\"send code failed; invalid account\",\"ret\":2112,\"seq\":\"" + seq + "\"}";

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
        var registerTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        JsonDb.Instance.Users.Add(new User()
        {
            ID = uid
        });

        dbContext.SdkUsers.Add(new SdkUser()
        {
            ID = uid,
            Email = req.account,
            PasswordHash = req.password,
            RegisterTime = registerTime,
            IsAdmin = admin,
            PlayerName = "Player_" + Rng.RandomString(8),
        });
        dbContext.Users.Add(new GameUser()
        {
            ID = uid // todo remove later
        });
        dbContext.SaveChanges();

        AccessToken tok = CreateLauncherTokenForUser(uid);

        return "{\"expire\":" + tok.ExpirationTime + ",\"is_login\":false,\"msg\":\"Success\",\"register_time\":" + registerTime + ",\"ret\":0,\"seq\":\"" + seq + "\",\"token\":\"" + tok.Token + "\",\"uid\":\"" + uid + "\"}";
    }
    public static AccessToken CreateLauncherTokenForUser(ulong id)
    {
        // TODO: implement access token expiration
        return new()
        {
            Token = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
        .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
        .AddClaim("userId", id)
        .IssuedAt(DateTime.UtcNow)
        //.Expiration(DateTime.UtcNow.AddDays(2))
        .Encode(),
            UserID = id,
            ExpirationTime = DateTime.UtcNow.AddDays(2).Ticks
        };
    }
}
