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
            var req = await ReadData<ReqGetGachaData>();
            var user = GetUser();

            var response = new ResGetGachaData();
            if (user.GachaTutorialPlayCount > 0)
                response.Gacha.Add(new NetUserGachaData() { GachaType = 3, PlayCount = 1 });
            WriteData(response);
        }
    }
}
