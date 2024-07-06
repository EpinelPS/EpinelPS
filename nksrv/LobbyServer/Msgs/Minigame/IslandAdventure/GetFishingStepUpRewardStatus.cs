using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/fishing/stepupreward")]
    public class GetFishingStepUpRewardStatus : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetIslandAdventureFishingStepUpRewardStatus>();

            var response = new ResGetIslandAdventureFishingStepUpRewardStatus();
            // TODO
            WriteData(response);
        }
    }
}
