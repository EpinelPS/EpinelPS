namespace EpinelPS.LobbyServer.Event.Minigame.Lycoris;

[GameRequest("/event/minigame/lycoris/getdata")]
public class GetLycorisData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqMiniGameLycorisDataInfo req = await ReadData<ReqMiniGameLycorisDataInfo>();
        User user = GetUser();

        ResMiniGameLycorisDataInfo response = new()
        {
            BaseData = new(),
            DailyTask = new(),
            Skills = new()
        };

        // TODO implement properly

        await WriteDataAsync(response);
    }
}