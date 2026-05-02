namespace EpinelPS.LobbyServer.Minigame.IslandAdventure;

[GameRequest("/event/minigame/islandadventure/get/photo/stepupreward")]
public class GetPhotoStepUpRewardStatus : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetIslandAdventurePhotoStepUpRewardStatus req = await ReadData<ReqGetIslandAdventurePhotoStepUpRewardStatus>();

        ResGetIslandAdventurePhotoStepUpRewardStatus response = new();
        // TODO
        await WriteDataAsync(response);
    }
}
