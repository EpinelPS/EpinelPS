namespace EpinelPS.LobbyServer.Character.Counsel;

[GameRequest("/character/counsel/check")]
public class CheckCounsel : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCounseledBefore req = await ReadData<ReqCounseledBefore>();

        ResCounseledBefore response = new();

        response.IsCounseledBefore = false;

        await WriteDataAsync(response);
    }
}