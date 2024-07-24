using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.IntlServer
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
    ""game_launcher_info"": [
        {
            ""id"": 27,
            ""execute_file"": ""NIKKE\\Game\\NIKKE.exe"",
            ""param"": """",
            ""description"": ""Nikke main process"",
            ""os"": ""any"",
            ""branch_id"": 0,
            ""status"": 1,
            ""param_type"": 1
        }
    ]
}", true);
        }
    }
}
