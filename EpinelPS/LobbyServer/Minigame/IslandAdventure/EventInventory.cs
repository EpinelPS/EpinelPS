namespace EpinelPS.LobbyServer.Minigame.IslandAdventure;

[GameRequest("/event/minigame/islandadventure/get/inventory")]
public class MiniGameIslandAdventureInventory : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameIslandAdventureInventory req = await ReadData<ReqGetMiniGameIslandAdventureInventory>();

        ResGetMiniGameIslandAdventureInventory response = new()
        {

        };

        await WriteDataAsync(response);
    }
}
