using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/fish/spotcount")]
    public class MiniGameIslandAdventureFishingSpotCountHistory : LobbyMsgHandler
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
}
