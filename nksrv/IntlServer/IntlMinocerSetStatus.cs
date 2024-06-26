using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;

namespace nksrv.IntlServer
{
    public class IntlMinocerSetStatus : IntlMsgHandler
    {
        public override bool RequiresAuth => true;

        protected override async Task HandleAsync()
        {
            var c = await ctx.GetRequestBodyAsStringAsync();
            throw new NotImplementedException();
        }
    }
}
