using EmbedIO;
using Newtonsoft.Json;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static nksrv.IntlServer.IntlLogin1Endpoint;
using static nksrv.IntlServer.IntlLogin2Endpoint;

namespace nksrv.IntlServer
{
    public class SendCodeEndpoint : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            SendCodeRequest? ep = JsonConvert.DeserializeObject<SendCodeRequest>(Content);
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

                // pretend that we sent the code
                await WriteJsonStringAsync("{\"expire_time\":898,\"msg\":\"Success\",\"ret\":0,\"seq\":\"" + seg + "\"}");
            }
            else
            {
                throw new HttpException(HttpStatusCode.BadRequest);
            }
        }

        public class SendCodeRequest
        {
            public DeviceInfo device_info { get; set; } = new();
            public string extra_json { get; set; } = "";
            public string account { get; set; } = "";
            public int account_type { get; set; }
            public string phone_area_code { get; set; } = "";
            public int code_type { get; set; }
            public int support_captcha { get; set; }
        }

    }
}
