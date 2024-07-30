using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Minigame.IslandAdventure
{
    [PacketPath("/event/minigame/islandadventure/list/mission")]
    public class ListMission : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetIslandAdventureMissionProgress>();

            var response = new ResGetIslandAdventureMissionProgress();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
