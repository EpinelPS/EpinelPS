namespace EpinelPS.LobbyServer.Minigame.IslandAdventure;

[GameRequest("/event/minigame/islandadventure/get/photo/album")]
public class MiniGameIslandAdventurePhotoAlbum : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqMiniGameIslandAdventurePhotoAlbum req = await ReadData<ReqMiniGameIslandAdventurePhotoAlbum>();

        ResMiniGameIslandAdventurePhotoAlbum response = new()
        {

        };

        await WriteDataAsync(response);
    }
}
