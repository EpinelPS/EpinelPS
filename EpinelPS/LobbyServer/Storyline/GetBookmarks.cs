namespace EpinelPS.LobbyServer.Storyline;

[GameRequest("/storyline/bookmark/get")]
public class GetBookmarks : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetStorylineBookmarks req = await ReadData<ReqGetStorylineBookmarks>();

        ResGetStorylineBookmarks response = new();
        User user = GetUser();

        // TODO

        await WriteDataAsync(response);
    }
}
