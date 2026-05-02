namespace EpinelPS.LobbyServer.Ranking;

[GameRequest("/ranking/updateserverreward")]
public class UpdateServerReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUpdateRankingServerReward req = await ReadData<ReqUpdateRankingServerReward>();
        ResUpdateRankingServerReward response = new();


        await WriteDataAsync(response);
    }
}
