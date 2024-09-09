using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/minigame/islandadventure/get/fish/spotcount")]
    public class MiniGameIslandAdventureFishingSpotCountHistory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqMiniGameIslandAdventureFishingSpotCountHistory>();

            var response = new ResMiniGameIslandAdventureFishingSpotCountHistory
            {

            };

            await WriteDataAsync(response);
        }
    }
}
