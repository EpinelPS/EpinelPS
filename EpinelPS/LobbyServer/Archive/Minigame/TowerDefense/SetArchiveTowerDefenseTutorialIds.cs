using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive.Minigame.TowerDefense;

[GameRequest("/archive/minigame/towerdefense/settutorialids")]
public class SetArchiveTowerDefenseTutorialIds : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetArchiveTowerDefenseTutorialIds req = await ReadData<ReqSetArchiveTowerDefenseTutorialIds>();
        User user = GetUser();

        if (!user.TowerDefenseDatas.TryGetValue(req.EventTowerDefenseArchiveManagerId, out var tdData))
        {
            tdData = new TowerDefenseData();
            user.TowerDefenseDatas[req.EventTowerDefenseArchiveManagerId] = tdData;
        }

        tdData.ClearedTutorialIdList.AddRangeUnique(req.TutorialIds);
        JsonDb.Save();

        ResSetArchiveTowerDefenseTutorialIds response = new();
        await WriteDataAsync(response);
    }
}
