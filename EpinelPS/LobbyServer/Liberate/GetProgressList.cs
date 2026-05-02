namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/getprogresslist")]
public class GetProgressList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetLiberateProgressList req = await ReadData<ReqGetLiberateProgressList>();
        User user = GetUser();

        ResGetLiberateProgressList response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
