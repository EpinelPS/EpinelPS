using EmbedIO;
using Newtonsoft.Json;
using nksrv.Database;
using System.Net;
using static nksrv.IntlServer.IntlLogin2Endpoint;

namespace nksrv.IntlServer
{
    public class IntlLogin1Endpoint : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            LoginEndpoint2Req? ep = JsonConvert.DeserializeObject<LoginEndpoint2Req>(Content);
            if (ep != null)
            {
                foreach (var item in JsonDb.Instance.Users)
                {
                    if (item.Username == ep.account && item.Password == ep.password)
                    {
                        var tok = IntlHandler.CreateLauncherTokenForUser(item);
                        item.LastLogin = DateTime.UtcNow;
                        JsonDb.Save();
                        await WriteJsonStringAsync("{\"expire\":" + tok.ExpirationTime + ",\"is_login\":true,\"msg\":\"Success\",\"register_time\":" + item.RegisterTime + ",\"ret\":0,\"seq\":\"" + Seq + "\",\"token\":\"" + tok.Token + "\",\"uid\":\"" + item.ID + "\"}");

                        return;
                    }
                }

                await WriteJsonStringAsync("{\"msg\":\"the account does not exists!\",\"ret\":2001,\"seq\":\"" + Seq + "\"}");
            }
            else
            {
                throw new HttpException(HttpStatusCode.BadRequest);
            }
        }

        public class LoginEndpoint2Req
        {
            public DeviceInfo device_info { get; set; } = new();
            public string extra_json { get; set; } = "";
            public string account { get; set; } = "";
            public int account_type { get; set; }
            public string password { get; set; } = "";
            public string phone_area_code { get; set; } = "";
            public int support_captcha { get; set; }
        }
    }
}
