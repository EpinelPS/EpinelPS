namespace EpinelPS.LobbyServer.Ranking;

[GameRequest("/ranking/alltoprank")]
public class GetTopRanks : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetAllTopRank req = await ReadData<ReqGetAllTopRank>();

        ResGetAllTopRank response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
