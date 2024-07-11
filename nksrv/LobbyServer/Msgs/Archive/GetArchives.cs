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
            response.ArchiveRecordList.AddRange([100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800]);
            response.UnlockedArchiveRecordList.AddRange([100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800]);
            // TODO: allow unlocking
            await WriteDataAsync(response);
        }
    }
}
