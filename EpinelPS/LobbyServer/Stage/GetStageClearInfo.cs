namespace EpinelPS.LobbyServer.Stage;

[GameRequest("/stageclearinfo/get")]
public class GetStageClearInfo : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetStageClearInfo req = await ReadData<ReqGetStageClearInfo>();
        ResGetStageClearInfo response = new();
        User user = GetUser();

        response.Historys.AddRange(user.StageClearHistorys);

        await WriteDataAsync(response);
    }
}
