namespace EpinelPS.LobbyServer.Minigame.IslandAdventure;

[GameRequest("/event/minigame/islandadventure/list/mission")]
public class ListMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetIslandAdventureMissionProgress req = await ReadData<ReqGetIslandAdventureMissionProgress>();

        ResGetIslandAdventureMissionProgress response = new();
        // TODO
        await WriteDataAsync(response);
    }
}
