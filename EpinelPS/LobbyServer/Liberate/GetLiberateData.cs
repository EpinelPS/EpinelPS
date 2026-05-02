namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/get")]
public class GetLiberateData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetLiberateData req = await ReadData<ReqGetLiberateData>();
        User user = GetUser();

        ResGetLiberateData response = new() { };

        // TODO

        await WriteDataAsync(response);
    }
}
