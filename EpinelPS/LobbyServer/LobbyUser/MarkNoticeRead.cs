namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/record/notice")]
public class MarkNoticeRead : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecordNoticeLog req = await ReadData<ReqRecordNoticeLog>();
        ResRecordNoticeLog r = new();
        User user = GetUser();

        // TODO

        await WriteDataAsync(r);
    }
}
