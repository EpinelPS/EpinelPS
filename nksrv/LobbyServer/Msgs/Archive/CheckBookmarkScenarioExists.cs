using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Archive
{
    [PacketPath("/bookmark/scenario/exist")]
    public class CheckBookmarkScenarioExists : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqExistScenarioBookmark>();

            var response = new ResExistScenarioBookmark();
            // TODO
            WriteData(response);
        }
    }
}
