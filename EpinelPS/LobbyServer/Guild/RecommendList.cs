namespace EpinelPS.LobbyServer.Guild;

[GameRequest("/guild/recommendlist")]
public class GetRecommendList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecommendGuildList req = await ReadData<ReqRecommendGuildList>();
        ResRecommendGuildList response = new();


        await WriteDataAsync(response);
    }
}
