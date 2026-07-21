using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.SortOut;

[GameRequest("/arcade/sortout/start")]
public class Start : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeSortOutStart req = await ReadData<ReqArcadeSortOutStart>();
        User user = GetUser();
        ResArcadeSortOutStart response = new();

        // TODO
        await WriteDataAsync(response);
    }
}