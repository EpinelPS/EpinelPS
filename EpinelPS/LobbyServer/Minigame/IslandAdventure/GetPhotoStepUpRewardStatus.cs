using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/photo/stepupreward")]
    public class GetPhotoStepUpRewardStatus : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetIslandAdventurePhotoStepUpRewardStatus req = await ReadData<ReqGetIslandAdventurePhotoStepUpRewardStatus>();

            ResGetIslandAdventurePhotoStepUpRewardStatus response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
