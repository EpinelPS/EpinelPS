namespace EpinelPS.LobbyServer.Misc;

[GameRequest("/featureflags/get")]
public class GetFeatureFlags : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetFeatureFlag req = await ReadData<ReqGetFeatureFlag>();

        ResGetFeatureFlag r = new()
        {
            IsOpen = true
        };

        await WriteDataAsync(r);
    }
}
