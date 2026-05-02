namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/minigame/getdata")]
public class GetMinigameData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArchiveMiniGameData req = await ReadData<ReqGetArchiveMiniGameData>();

        ResGetArchiveMiniGameData response = new()
        {
            Json = "{}"
        };
        // TODO

        await WriteDataAsync(response);
    }
}
