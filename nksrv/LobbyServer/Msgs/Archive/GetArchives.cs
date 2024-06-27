using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Archive
{
    [PacketPath("/archive/get")]
    public class GetArchives : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArchiveRecord>();

            var response = new ResGetArchiveRecord();
            // TODO
            WriteData(response);
        }
    }
}
