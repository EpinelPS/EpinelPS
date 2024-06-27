using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Arena
{
    [PacketPath("/arena/getbaninfo")]
    public class GetArenaBanInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArenaBanInfo>();

            var response = new ResGetArenaBanInfo();
            // TODO
            WriteData(response);
        }
    }
}
