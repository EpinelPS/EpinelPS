using System.Threading.Tasks;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Guild;

[GameRequest("/shootingrange/v2/get")]
public class ShootingRangeGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetShootingRangeRankingV2>();
        User user = GetUser();

        ResGetShootingRangeRankingV2 response = new()
        {
            Result = ShootingRangeRankingResult.GetShootingRangeRankingResultSuccess,
            IsBan = false
        };

        var wholeUser = LobbyHandler.CreateWholeUserDataFromDbUser(user);

        // 15 distinct (Element, Distance) modes
        int[] rangeIds = [1, 3, 21, 5, 7, 23, 9, 11, 25, 13, 15, 27, 17, 19, 29];

        foreach (int rangeId in rangeIds)
        {
            var totalData = new NetShootingRangeRankingTotalData
            {
                ShootingRangeId = rangeId,
                UserGuildRanking = new NetShootingRangeRankingData
                {
                    Rank = 1,
                    Score = 0,
                    User = wholeUser
                }
            };

            totalData.GuildRankingList.Add(new NetShootingRangeRankingData
            {
                Rank = 1,
                Score = 0,
                User = wholeUser
            });

            response.RankingList.Add(totalData);
        }

        await WriteDataAsync(response);
    }
}

[GameRequest("/shootingrange/v2/set")]
public class ShootingRangeSet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetShootingRangeScoreV2 req = await ReadData<ReqSetShootingRangeScoreV2>();

        ResSetShootingRangeScoreV2 response = new()
        {
            IsNewRecord = true,
            HighestScore = req.Score,
            Result = ShootingRangeRankingResult.GetShootingRangeRankingResultSuccess,
            IsBan = false
        };

        await WriteDataAsync(response);
    }
}

[GameRequest("/shootingrange/getbattlelog")]
public class ShootingRangeBattleLogGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetShootingRangeBattleLog>();
        ResGetShootingRangeBattleLog response = new();
        await WriteDataAsync(response);
    }
}
