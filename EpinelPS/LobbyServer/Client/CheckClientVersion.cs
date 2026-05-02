namespace EpinelPS.LobbyServer.Client;

[GameRequest("/system/checkversion")]
public class CheckClientVersion : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCheckClientVersion req = await ReadData<ReqCheckClientVersion>();
        ResCheckClientVersion response = new()
        {
            Availability = ResCheckClientVersion.Types.Availability.None
        };

        await WriteDataAsync(response);
    }
}
