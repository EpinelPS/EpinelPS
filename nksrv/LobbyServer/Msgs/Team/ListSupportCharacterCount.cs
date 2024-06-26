using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Team
{
    [PacketPath("/team/support-character/list-used-count")]
    public class ListSupportCharacterCount : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListSupportCharacterUsedCount>();

            var response = new ResListSupportCharacterUsedCount();
            foreach (var item in req.TeamTypeList)
            {
                Console.WriteLine("support character used: " + item);
            }
            WriteData(response);
        }
    }
}
