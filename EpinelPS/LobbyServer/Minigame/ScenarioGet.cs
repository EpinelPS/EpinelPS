using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame;

[GameRequest("/arcade/scenario/get")]
public class ScenarioGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeScenario req = await ReadData<ReqGetArcadeScenario>();
        User user = GetUser();
        ResGetArcadeScenario response = new();
        if (user.MiniGameScenarios.TryGetValue(req.ArcadeId, out MiniGameScenarios? scenarios))
        {
            response.ArcadeScenarioIdList.AddRange(scenarios.CompletedScenarios);
            //response.AlbumScenarioIdListExceptArcade
        }
               

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}