using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/photo/stepupreward")]
    public class GetPhotoStepUpRewardStatus : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetIslandAdventurePhotoStepUpRewardStatus>();

            var response = new ResGetIslandAdventurePhotoStepUpRewardStatus();
            // TODO
            WriteData(response);
        }
    }
}
