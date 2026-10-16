using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Badge;

[GameRequest("/badge/record/content")]
public class RecordContent : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqRecordContentEnter req = await ReadData<ReqRecordContentEnter>();
        User user = GetUser();

        ResRecordContentEnter response = new();

        await WriteDataAsync(response);
    }
}
