namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/lobby/usertitle/acquire")]
public class AquireUserTitle : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAcquireUserTitle req = await ReadData<ReqAcquireUserTitle>();
        User user = GetUser();

        ResAcquireUserTitle response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
