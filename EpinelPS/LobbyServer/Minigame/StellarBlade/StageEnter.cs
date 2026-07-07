using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/stage/enter")]
public class StageEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeEnterStellarBladeStage req = await ReadData<ReqArcadeEnterStellarBladeStage>();
        User user = GetUser();
        ResArcadeEnterStellarBladeStage response = new();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            stellar.LastEnteredStageId = req.StageId;
        }

        JsonDb.Save();     
        // TODO
        await WriteDataAsync(response);
    }
}