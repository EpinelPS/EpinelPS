using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Ranking/Song/Get")]
public class SongRankingGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameTtsSongRanking req = await ReadData<ReqGetMiniGameTtsSongRanking>();
        User user = GetUser();
        ResGetMiniGameTtsSongRanking response = new();

        var entities = TtsHelper.GetBySongIdAndRankTypeWithRank(req.EventTtsSongManagerTableId, req.RankingType);
        if (entities.Count > 0)
        {
            foreach (var item in entities)
            {
                NetMiniGameTtsSongRankData data = new()
                {
                    Difficulty = item.Entity.Difficulty,
                    Position = item.Rank,
                    Score = item.Entity.Score,
                    User = TtsHelper.CreateWholeUserDataFromDbUser(item.Entity.UserId)
                };
                response.TopRankDataList.Add(data);
            }

        }

        List<(SongRankData Entity, int Rank)>? srank = TtsHelper
                .GetUserSongRankingsWithRank((long)user.ID, req.EventTtsSongManagerTableId, MiniGameTtsRankingType.Server);

        NetMyMiniGameTtsSongRankData rankData = new() { Difficulty = MiniGameTtsDifficulty.Casual, Position = 0, Score = 0 };
        if (srank.Count > 0)
        {
            var myfrank = srank.OrderBy(x => x.Rank).First();
            response.MyRankData = new NetMyMiniGameTtsSongRankData() { Difficulty = myfrank.Entity.Difficulty, Position = myfrank.Rank, Score = myfrank.Entity.Score };
        }
        else
        {
            response.MyRankData = rankData;
        }        
        
        await WriteDataAsync(response);
    }
}