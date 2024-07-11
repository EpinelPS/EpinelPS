using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Minigame
{
    [PacketPath("/minigame/nksv2/get")]
    public class GetNksv2Minigame : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMiniGameNKSV2Data>();

            var response = new ResGetMiniGameNKSV2Data();
            // TODO
          await  WriteDataAsync(response);
        }
    }
}
