using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Song/Get")]
public class SongGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameTtsSongData req = await ReadData<ReqGetMiniGameTtsSongData>();
        User user = GetUser();
        ResGetMiniGameTtsSongData response = new();

        //Logging.WriteLine($"{req.EventTtsManagerTableId},{req.EventTtsSongManagerTableId}", LogType.Info);
        

        var song = GameData.Instance.EventTTSSongManagerTable.Values
            .Where(s=>s.Id == req.EventTtsSongManagerTableId).FirstOrDefault();

        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
                       

            List<(SongRankData Entity, int Rank)>? frank = TtsHelper
                .GetUserSongRankingsWithRank((long)user.ID,req.EventTtsSongManagerTableId, MiniGameTtsRankingType.Friend);

            List<(SongRankData Entity, int Rank)>? urank = TtsHelper
                .GetUserSongRankingsWithRank((long)user.ID, req.EventTtsSongManagerTableId, MiniGameTtsRankingType.Union);

            NetMyMiniGameTtsSongRankData rankData = new() { Difficulty = MiniGameTtsDifficulty.Casual, Position = 0, Score = 0 };
            if (frank.Count >0)
            {
                var myfrank = frank.OrderBy(x => x.Rank).First();
                response.MyFriendSongRankData = new NetMyMiniGameTtsSongRankData() { Difficulty = myfrank.Entity.Difficulty, Position = myfrank.Rank, Score = myfrank.Entity.Score };
            }
            else
            {
                response.MyFriendSongRankData = rankData;
            }

            if (urank.Count > 0 && user.Guild.guildId >0)
            {
                var myurank = urank.OrderBy(x => x.Rank).First();
                response.MyUnionSongRankData = new NetMyMiniGameTtsSongRankData() { Difficulty = myurank.Entity.Difficulty, Position = myurank.Rank, Score = myurank.Entity.Score };
            }
            else
            {
                response.MyUnionSongRankData = rankData;
            }
            //response.NewBadgeEventTtsSongManagerTableIds.AddRange(ttsData.BadgeSongId);

            if (ttsData.ScoreData.Count>0)
            {
                response.ScoreData.AddRange(ttsData.ScoreData);
            }
            else 
            {
                response.ScoreData.Add(new NetMiniGameTtsScoreData() 
                {
                    Difficulty = MiniGameTtsDifficulty.Casual,
                    EventTtsSongManagerTableId = req.EventTtsSongManagerTableId,
                    Score = 0
                });
            }    
            
        }
        

        
        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}