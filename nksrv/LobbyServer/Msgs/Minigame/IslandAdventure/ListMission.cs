using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/list/mission")]
    public class ListMission : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetIslandAdventureMissionProgress>();

            var response = new ResGetIslandAdventureMissionProgress();
            // TODO
            WriteData(response);
        }
    }
}
