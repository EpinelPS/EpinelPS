using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser.Mail;

[GameRequest("/mail/read")]
public class ReadMail : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqReadMail req = await ReadData<ReqReadMail>();
        //GameUser user = GetUser();
        ResReadMail response = new();        

        //TODO
        await WriteDataAsync(response);
    }
}