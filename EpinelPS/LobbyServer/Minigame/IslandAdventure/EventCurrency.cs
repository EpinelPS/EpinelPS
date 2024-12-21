using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/minigame/islandadventure/get/currency")]
    public class MiniGameIslandAdventureCurrency : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMiniGameIslandAdventureCurrency>();

            var response = new ResGetMiniGameIslandAdventureCurrency
            {
                Currency = 90000  
            };

            await WriteDataAsync(response);
        }
    }
}
