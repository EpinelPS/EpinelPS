namespace EpinelPS.LobbyServer.Episode;

[GameRequest("/episode/mission/enter")]
public class ListMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListValidEpMission req = await ReadData<ReqListValidEpMission>();

        ResListValidEpMission response = new();

        // TOOD

        await WriteDataAsync(response);
    }
}
