namespace EpinelPS.LobbyServer.Minigame.IslandAdventure;

[GameRequest("/event/minigame/islandadventure/get/fish/spotcount")]
public class MiniGameIslandAdventureFishingSpotCountHistory : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqMiniGameIslandAdventureFishingSpotCountHistory req = await ReadData<ReqMiniGameIslandAdventureFishingSpotCountHistory>();

        ResMiniGameIslandAdventureFishingSpotCountHistory response = new()
        {

        };

        await WriteDataAsync(response);
    }
}
