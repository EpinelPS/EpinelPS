using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/cleartutorial")]
public class ClearTutorial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArcadeTowerDefenseTutorial req = await ReadData<ReqClearArcadeTowerDefenseTutorial>();
        User user = GetUser();
        ResClearArcadeTowerDefenseTutorial response = new();

        if (user.TowerDefenseDatas.TryGetValue(req.ArcadeManagerId, out TowerDefenseData? data))
        {
            data.ClearedTutorialIdList.AddRangeUnique(req.TutorialIds);
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}