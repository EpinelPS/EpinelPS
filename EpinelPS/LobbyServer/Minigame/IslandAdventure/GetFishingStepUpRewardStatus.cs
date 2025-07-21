using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/fishing/stepupreward")]
    public class GetFishingStepUpRewardStatus : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetIslandAdventureFishingStepUpRewardStatus req = await ReadData<ReqGetIslandAdventureFishingStepUpRewardStatus>();

            ResGetIslandAdventureFishingStepUpRewardStatus response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
