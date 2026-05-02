namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/memorial/getmemorylist")]
public class MemorialGetMemoryList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMemoryList req = await ReadData<ReqGetMemoryList>();
        User user = GetUser();

        ResGetMemoryList response = new();

        response.MemoryList.AddRange(user.Memorial);

        // TODO rewards

        await WriteDataAsync(response);
    }
}
