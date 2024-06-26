using Google.Protobuf.WellKnownTypes;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs
{
    [PacketPath("/Gacha/Get")]
    public class GetGacha : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = ReadData<ReqGetGachaData>();

            var response = new ResGetGachaData();

            WriteData(response);
        }
    }
}
