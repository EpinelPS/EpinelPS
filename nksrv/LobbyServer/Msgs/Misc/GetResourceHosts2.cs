using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Misc
{
    [PacketPath("/resourcehosts2")]
    public class CheckClientVersion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ResourceHostRequest>();

            var r = new ResourceHostResponse();
            r.BaseUrl = GameConfig.Root.ResourceBaseURL;

            await WriteDataAsync(r);
        }
    }
}
