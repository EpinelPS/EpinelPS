using EmbedIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static nksrv.IntlServer.IntlLogin2Endpoint;
using Newtonsoft.Json;
using static nksrv.IntlServer.IntlLogin1Endpoint;
using nksrv.Utils;

namespace nksrv.IntlServer
{
    public class IntlAccountRegisterEndpoint : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {

            RegisterEPReq? ep = JsonConvert.DeserializeObject<RegisterEPReq>(Content);
            if (ep != null)
            {
                string? seg = ctx.GetRequestQueryData().Get("seq");

                // check if the account already exists
                foreach (var item in JsonDb.Instance.Users)
                {
                    if (item.Username == ep.account)
                    {
                        await WriteJsonStringAsync("{\"msg\":\"send code failed; invalid account\",\"ret\":2112,\"seq\":\"" + seg + "\"}");
                        return;
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

                var user = new User() { ID = uid, Password = ep.password, RegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Username = ep.account, PlayerName = "Player_"+Rng.RandomString(8) };

                JsonDb.Instance.Users.Add(user);

                var tok = IntlHandler.CreateLauncherTokenForUser(user);
                await WriteJsonStringAsync("{\"expire\":" + tok.ExpirationTime + ",\"is_login\":false,\"msg\":\"Success\",\"register_time\":" + user.RegisterTime + ",\"ret\":0,\"seq\":\"" + seg + "\",\"token\":\"" + tok.Token + "\",\"uid\":\"" + user.ID + "\"}");
            }
            else
            {
                throw new HttpException(HttpStatusCode.BadRequest);
            }
        }

        public class RegisterEPReq
        {
            public DeviceInfo device_info { get; set; } = new();
            public string verify_code { get; set; } = "";
            public string account { get; set; } = "";
            public int account_type { get; set; }
            public string phone_area_code { get; set; } = "";
            public string password { get; set; } = "";
            public string user_name { get; set; } = "";
            public string birthday { get; set; } = "";
            public string region { get; set; } = "";
            public string user_lang_type { get; set; } = "";
            public string extra_json { get; set; } = "";
        }
    }
}
