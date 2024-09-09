using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/event/minigame/islandadventure/get/inventory")]
    public class MiniGameIslandAdventureInventory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMiniGameIslandAdventureInventory>();

            var response = new ResGetMiniGameIslandAdventureInventory
            {

            };

            await WriteDataAsync(response);
        }
    }
}
