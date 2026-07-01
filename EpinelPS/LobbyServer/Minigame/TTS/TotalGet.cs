using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Ranking/Total/Get")]
public class TotalGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameTtsTotalRanking req = await ReadData<ReqGetMiniGameTtsTotalRanking>();
        User user = GetUser();
        ResGetMiniGameTtsTotalRanking response = new();
        RankData rank = GetRank();

        List<NetMiniGameTtsTotalRankData>? entities = rank.TtsRankDatas.TotalGetLeaderboardByRankType(req.RankingType);
        NetMiniGameTtsTotalRankData? myrank = rank.TtsRankDatas.TotalGetUserRank((long)user.ID, req.RankingType);


        if (entities.Count > 0)
        {
            response.TopRankDataList.AddRange(entities);
        }

        if (myrank != null)
        {
            NetMyMiniGameTtsTotalRankData mrank = new()
            {
                Position = myrank.Position,
                Score = myrank.Score
            };

            response.MyRankData = mrank;
        }
        await WriteDataAsync(response);
    }
}