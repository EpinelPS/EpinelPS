using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.Lycoris;

[PacketPath("/event/minigame/lycoris/getdata")]
public class GetLycorisData : LobbyMsgHandler
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