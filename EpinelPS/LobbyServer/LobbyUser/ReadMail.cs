namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/mail/read")]
public class ReadMail : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqReadMail req = await ReadData<ReqReadMail>();

        ResReadMail r = new();
        //TODO
        await WriteDataAsync(r);
    }
}
