using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Profile/Get")]
public class ProfileGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameTtsProfile req = await ReadData<ReqGetMiniGameTtsProfile>();
        User user = GetUser();
        ResGetMiniGameTtsProfile response = new();
        RankData rank = GetRank();

        NetMyMiniGameTtsTotalRankData mytotal = new(); 
        NetMiniGameTtsTotalRankData? myrank = rank.TtsRankDatas.TotalGetUserRank((long)user.ID, MiniGameTtsRankingType.Server);

        if (myrank!=null)
        {
            mytotal.Score = myrank.Score;
            mytotal.Position = myrank.Position;
            
        }
        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {            
            var songPlayList = ttsData.SongPlayCount
                .Select(m => MiniGameHelper.ToProto<NetMiniGameTtsSongPlayCount, MiniGameTtsSongPlayCount>(m))
                .ToList();

            response.PlayCountList.AddRange(songPlayList);            
        }

        response.MyServerRankData = mytotal;
        await WriteDataAsync(response);
    }
}