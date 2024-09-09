using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/minigame/islandadventure/get/photo/album")]
    public class MiniGameIslandAdventurePhotoAlbum : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqMiniGameIslandAdventurePhotoAlbum>();

            var response = new ResMiniGameIslandAdventurePhotoAlbum
            {

            };

            await WriteDataAsync(response);
        }
    }
}
