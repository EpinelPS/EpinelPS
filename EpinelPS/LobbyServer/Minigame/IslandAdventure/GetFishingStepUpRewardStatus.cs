namespace EpinelPS.LobbyServer.Minigame.IslandAdventure;

[GameRequest("/event/minigame/islandadventure/get/fishing/stepupreward")]
public class GetFishingStepUpRewardStatus : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetIslandAdventureFishingStepUpRewardStatus req = await ReadData<ReqGetIslandAdventureFishingStepUpRewardStatus>();

        ResGetIslandAdventureFishingStepUpRewardStatus response = new();
        // TODO
        await WriteDataAsync(response);
    }
}
