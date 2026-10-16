namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/newbiepackage/get")]
public class GetNewbiePackage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetNewbiePackage>();

        ResGetNewbiePackage response = new();
        await WriteDataAsync(response);
    }
}
