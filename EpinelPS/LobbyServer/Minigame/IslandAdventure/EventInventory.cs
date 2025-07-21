using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/get/inventory")]
    public class MiniGameIslandAdventureInventory : LobbyMsgHandler
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
}
