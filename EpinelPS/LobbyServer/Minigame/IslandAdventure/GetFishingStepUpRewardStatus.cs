﻿using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/fishing/stepupreward")]
    public class GetFishingStepUpRewardStatus : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetIslandAdventureFishingStepUpRewardStatus>();

            var response = new ResGetIslandAdventureFishingStepUpRewardStatus();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
