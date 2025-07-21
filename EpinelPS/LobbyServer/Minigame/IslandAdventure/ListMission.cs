using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/list/mission")]
    public class ListMission : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetIslandAdventureMissionProgress req = await ReadData<ReqGetIslandAdventureMissionProgress>();

            ResGetIslandAdventureMissionProgress response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
