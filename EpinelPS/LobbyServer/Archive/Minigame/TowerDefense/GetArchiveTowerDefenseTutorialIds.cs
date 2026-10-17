using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Archive.Minigame.TowerDefense;

[GameRequest("/archive/minigame/towerdefense/gettutorialids")]
public class GetArchiveTowerDefenseTutorialIds : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArchiveTowerDefenseTutorialIds req = await ReadData<ReqGetArchiveTowerDefenseTutorialIds>();
        User user = GetUser();

        ResGetArchiveTowerDefenseTutorialIds response = new();

        if (user.TowerDefenseDatas.TryGetValue(req.EventTowerDefenseArchiveManagerId, out var tdData))
        {
            response.TutorialIds.AddRange(tdData.ClearedTutorialIdList);
        }

        await WriteDataAsync(response);
    }
}
