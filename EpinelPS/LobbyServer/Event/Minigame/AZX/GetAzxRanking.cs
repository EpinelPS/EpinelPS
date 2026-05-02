namespace EpinelPS.LobbyServer.Event.Minigame.AZX;

[GameRequest("/event/minigame/azx/get/ranking")]
public class GetAzxRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // int AzxId
        ReqGetMiniGameAzxRanking req = await ReadData<ReqGetMiniGameAzxRanking>();
        User user = GetUser();

        ResGetMiniGameAzxRanking response = new();

        if (req.AzxId > 0)
            AzxHelper.GetRanking(user, req.AzxId, ref response);


        await WriteDataAsync(response);
    }
}