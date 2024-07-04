using EmbedIO;
using Newtonsoft.Json;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                        WriteJsonString("{\"expire\":" + tok.ExpirationTime + ",\"is_login\":true,\"msg\":\"Success\",\"register_time\":" + item.RegisterTime + ",\"ret\":0,\"seq\":\"" + Seq + "\",\"token\":\"" + tok.Token + "\",\"uid\":\"" + item.ID + "\"}");

                        return;
                    }
                }

                string? seg = ctx.GetRequestQueryData().Get("seq");

                WriteJsonString("{\"msg\":\"the account does not exists!\",\"ret\":2001,\"seq\":\"" + seg + "\"}");
            }
            else
            {
                throw new HttpException(HttpStatusCode.BadRequest);
            }
        }

        public class LoginEndpoint2Req
        {
            public DeviceInfo device_info { get; set; }
            public string extra_json { get; set; }
            public string account { get; set; }
            public int account_type { get; set; }
            public string password { get; set; }
            public string phone_area_code { get; set; }
            public int support_captcha { get; set; }
        }
    }
}
