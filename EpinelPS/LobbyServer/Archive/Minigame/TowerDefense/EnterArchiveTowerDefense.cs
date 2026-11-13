using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Archive.Minigame.TowerDefense;

[GameRequest("/archive/minigame/towerdefense/enter")]
public class EnterArchiveTowerDefense : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArchiveTowerDefense req = await ReadData<ReqEnterArchiveTowerDefense>();
        User user = GetUser();

        if (!user.TowerDefenseDatas.TryGetValue(req.EventId, out var tdData))
        {
            tdData = new TowerDefenseData();
            user.TowerDefenseDatas[req.EventId] = tdData;
        }

        tdData.LastEnteredStageId = req.StageId;

        ResEnterArchiveTowerDefense response = new();
        await WriteDataAsync(response);
    }
}
