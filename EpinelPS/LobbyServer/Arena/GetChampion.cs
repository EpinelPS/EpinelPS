namespace EpinelPS.LobbyServer.Arena;

[GameRequest("/arena/champion/get")]
public class GetChampion : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetChampionArena req = await ReadData<ReqGetChampionArena>();

        ResGetChampionArena response = new()
        {
            Schedule = new NetChampionArenaSchedule()
        };

        // TODO

        await WriteDataAsync(response);
    }
}
