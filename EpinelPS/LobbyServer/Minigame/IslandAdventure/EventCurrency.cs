namespace EpinelPS.LobbyServer.Minigame.IslandAdventure;

[GameRequest("/event/minigame/islandadventure/get/currency")]
public class MiniGameIslandAdventureCurrency : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameIslandAdventureCurrency req = await ReadData<ReqGetMiniGameIslandAdventureCurrency>();

        ResGetMiniGameIslandAdventureCurrency response = new()
        {
            Currency = 90000
        };

        await WriteDataAsync(response);
    }
}
