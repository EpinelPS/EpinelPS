namespace EpinelPS.LobbyServer.Archive.Minigame.IslandAdventure;

[GameRequest("/archive/minigame/islandadventure/get/all")]
[GameRequest("/archive/minigame/islandadventure/getall")]
[GameRequest("/archive/minigame/islandadventure/getalldata")]
[GameRequest("/archive/minigame/islandadventure/get/alldata")]
[GameRequest("/minigame/islandadventure/get/all")]
[GameRequest("/minigame/islandadventure/getall")]
[GameRequest("/minigame/islandadventure/getalldata")]
public class ArchiveIslandAdventureGetAllData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveIslandAdventureGetAllData>();

        ResArchiveIslandAdventureGetAllData response = new()
        {
            InventoryData = new ResArchiveGetMiniGameIslandAdventureInventory(),
            FishingSpotCountData = new ResArchiveMiniGameIslandAdventureFishingSpotCountHistory(),
            CurrencyData = new ResArchiveGetMiniGameIslandAdventureCurrency { Currency = 90000 },
            MissionData = new ResGetArchiveIslandAdventureMissionData(),
            PhotoAlbumData = new ResArchiveMiniGameIslandAdventurePhotoAlbum()
        };

        await WriteDataAsync(response);
    }
}
