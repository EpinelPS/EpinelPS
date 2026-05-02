namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/getfieldtalklist")]
public class GetFieldTalkList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetFieldTalkList req = await ReadData<ReqGetFieldTalkList>();
        User user = GetUser();

        ResGetFieldTalkList response = new();
        // TODO

        await WriteDataAsync(response);
    }
}
