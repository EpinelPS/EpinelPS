using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/photo/album")]
    public class MiniGameIslandAdventurePhotoAlbum : LobbyMsgHandler
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
}
