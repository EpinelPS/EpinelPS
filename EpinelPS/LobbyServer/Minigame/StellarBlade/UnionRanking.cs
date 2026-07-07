using EpinelPS.Utils;


namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/union-ranking")]
public class UnionRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeGetStellarBladeUnionRanking req = await ReadData<ReqArcadeGetStellarBladeUnionRanking>();
        User user = GetUser();
        ResArcadeGetStellarBladeUnionRanking response = new();

        IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = MiniGameHelper.GetArcadeBoard(req.ArcadeManagerId);

        if (allBoard.Count() > 0)
        {
            foreach (var item in allBoard)
            {

                NetMiniGameStellarBladeRankingData? rankdata = new NetMiniGameStellarBladeRankingData()
                {
                    Rank = item.Rank,
                    Score = item.Record.Score,
                    User = LobbyHandler.CreateWholeUserDataFromDbUser((ulong)item.Record.UserId)
                };

                response.RankingDataList.Add(rankdata);
                if (item.Record.UserId == user.ID)
                {
                    response.UserRankingData = rankdata;
                }

            }
        }
        else
        {
            response.UserRankingData = new() { Rank = 99, Score = 0, User = LobbyHandler.CreateWholeUserDataFromDbUser((ulong)user.ID) };
        }       

            
        // TODO
        await WriteDataAsync(response);
    }
}