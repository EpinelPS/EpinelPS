using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/statistics")]
public class Statistics : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeDragonDungeonRunStatistics req = await ReadData<ReqGetArcadeDragonDungeonRunStatistics>();
        User user = GetUser();
        ResGetArcadeDragonDungeonRunStatistics response = new();

        foreach (var item in user.DDRDatas.StatisticsDatas)
        {
            var statistics = new ResGetArcadeDragonDungeonRunStatistics.Types.NetArcadeDragonDungeonRunStatistics();

            if (item.Value.DeadCountList.Count > 0)
            {
                foreach (var deadcut in item.Value.DeadCountList)
                {
                    statistics.DeadCountList.Add(new ResGetArcadeDragonDungeonRunStatistics.Types.NetArcadeDragonDungeonRunStatistics.Types.NetDeadCount()
                    {
                        DeadCutScenePrintType = deadcut.Key,
                        Count = deadcut.Value
                    });
                }
            }

            statistics.CharacterId = item.Key;
            statistics.CumulativeDistance = item.Value.CumulativeDistance;
            statistics.CumulativeGold = item.Value.CumulativeGold;
            statistics.MaxPoint = item.Value.Score;
            statistics.PlayCount = item.Value.PlayCount;

            response.StatisticsList.Add(statistics);
        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}