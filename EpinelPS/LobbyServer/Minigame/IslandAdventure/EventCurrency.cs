using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/currency")]
    public class MiniGameIslandAdventureCurrency : LobbyMsgHandler
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
}
