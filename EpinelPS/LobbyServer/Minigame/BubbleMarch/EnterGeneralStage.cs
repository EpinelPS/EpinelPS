using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/entergeneralstage")]
public class EnterGeneralStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArcadeBubbleMarchGeneralStage req = await ReadData<ReqEnterArcadeBubbleMarchGeneralStage>();
        User user = GetUser();
        ResEnterArcadeBubbleMarchGeneralStage response = new();   

        // TODO
        await WriteDataAsync(response);
    }
}