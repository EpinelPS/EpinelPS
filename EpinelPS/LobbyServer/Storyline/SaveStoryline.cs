namespace EpinelPS.LobbyServer.Storyline;

[GameRequest("/storyline/save")]
public class SaveStoryline : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveRecentStoryline req = await ReadData<ReqSaveRecentStoryline>();

        ResGetStorylineData response = new();
        User user = GetUser();

        // TODO

        await WriteDataAsync(response);
    }
}
