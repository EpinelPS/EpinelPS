using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/statistics/get")]
public class StatisticsGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeGetStellarBladeStatistics req = await ReadData<ReqArcadeGetStellarBladeStatistics>();
        User user = GetUser();
        ResArcadeGetStellarBladeStatistics response = new();

        ResArcadeGetStellarBladeStatistics.Types.NetStatisticsData netStatistics = new();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            if (stellar.StatisticsData.Count > 0)
            {
                var statistdata = MiniGameHelper
                    .ToProtoDict<int, ResArcadeGetStellarBladeStatistics.Types.NetStatisticsData, StatisticsData>(stellar.StatisticsData);
                response.StatisticsList.AddRange(statistdata.Values); 
            }
        }
        // TODO
        await WriteDataAsync(response);
    }
}