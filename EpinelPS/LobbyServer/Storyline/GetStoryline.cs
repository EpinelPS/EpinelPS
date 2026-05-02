namespace EpinelPS.LobbyServer.Storyline;

[GameRequest("/storyline/get")]
public class GetStoryline : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetStorylineData req = await ReadData<ReqGetStorylineData>();

        ResGetStorylineData response = new();
        User user = GetUser();

        // TODO

        await WriteDataAsync(response);
    }
}
