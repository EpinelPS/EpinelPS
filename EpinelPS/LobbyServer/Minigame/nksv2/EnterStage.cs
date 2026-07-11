using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/enterstage")]
public class EnterStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterMiniGameNKSV2Stage req = await ReadData<ReqEnterMiniGameNKSV2Stage>();
        User user = GetUser();
        ResEnterMiniGameNKSV2Stage response = new();

        //Logging.WriteLine($"{req.NKsId}", LogType.Info);

        user.AddTrigger(Trigger.EventMiniGameNKSPlayCheck, 1, 0);

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}