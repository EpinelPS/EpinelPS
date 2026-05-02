namespace EpinelPS.LobbyServer.Soloraid;

[GameRequest("/soloraid/getranking")]
public class GetRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetSoloRaidRanking>();

        ResGetSoloRaidRanking response = new();

        // TODO

        await WriteDataAsync(response);
    }
}