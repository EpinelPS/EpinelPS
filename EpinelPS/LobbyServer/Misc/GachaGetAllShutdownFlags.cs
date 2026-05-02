namespace EpinelPS.LobbyServer.Misc;

[GameRequest("/shutdownflags/gacha/getall")]
public class GachaGetAllShutdownFlags : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGachaGetAllShutdownFlags req = await ReadData<ReqGachaGetAllShutdownFlags>();
        User user = GetUser();

        ResGachaGetAllShutdownFlags response = new();
        if (user.GachaTutorialPlayCount > 0)
            response.Unavailables.Add(3);

        // TODO: Validate response from real server and pull info from user info
        await WriteDataAsync(response);
    }
}
