using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/enterstage")]
public class EnterStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArcadeTowerDefenseStage req = await ReadData<ReqEnterArcadeTowerDefenseStage>();
        User user = GetUser();
        ResEnterArcadeTowerDefenseStage response = new();


        if (user.TowerDefenseDatas.TryGetValue(req.ArcadeManagerId, out TowerDefenseData? data))
        {
            data.LastEnteredStageId = req.StageId;
        }

        user.AddTrigger(Trigger.EventTowerDefensePlayCheck, 1, req.StageId);

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}