using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.IntlServer
{
    internal class JuniperLauncherGetRepoVersion : IntlMsgHandler
    {
        public override bool RequiresAuth => false;

        protected override async Task HandleAsync()
        {
            await WriteJsonStringAsync(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gameversion.json")), true);
        }
    }
}
