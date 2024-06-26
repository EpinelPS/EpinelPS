using EmbedIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.IntlServer
{
    public class CodeStatusEndpoint : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {

            // pretend that any code is valid

            WriteJsonString("{\"expire_time\":759,\"msg\":\"Success\",\"ret\":0,\"seq\":\""+Seq+"\"}");

        }
    }
}
