using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Minigame.Dave
{
    [PacketPath("/event/minigame/dave/getalldavetrigger")]
    public class GetAllDaveTrigger : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetAllMiniGameDaveTriggers>();

            var response = new ResGetAllMiniGameDaveTriggers();
            // TODO
          await  WriteDataAsync(response);
        }
    }
}
