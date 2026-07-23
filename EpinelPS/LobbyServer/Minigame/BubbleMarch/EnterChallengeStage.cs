using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/enterchallengestage")]
public class EnterChallengeStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArcadeBubbleMarchChallengeStage req = await ReadData<ReqEnterArcadeBubbleMarchChallengeStage>();
        User user = GetUser();
        ResEnterArcadeBubbleMarchChallengeStage response = new();

        // TODO
        await WriteDataAsync(response);
    }
}