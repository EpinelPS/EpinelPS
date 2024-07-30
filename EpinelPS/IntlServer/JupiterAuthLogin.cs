using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.IntlServer
{
    internal class JupiterAuthLogin : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            await WriteJsonStringAsync(@"{
    ""result"": {
        ""error_code"": 0,
        ""error_message"": ""COMM_SUCC""
    },
    ""channel"": 0,
    ""game_id"": ""0"",
    ""openid"": """",
    ""uid"": """",
    ""biz_ticket"": """",
    ""expire_interval"": 0,
    ""refresh_interval"": 0,
    ""login_key"": """",
    ""login_ticket"": """",
    ""third_uid"": """"
}", true);
        }
    }
}
