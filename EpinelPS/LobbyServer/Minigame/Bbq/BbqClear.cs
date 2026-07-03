using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.Bbq;

[GameRequest("/arcade/bbq/clear")]
public class BbqClear : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArcadeBBQ req = await ReadData<ReqClearArcadeBBQ>();
        User user = GetUser();
        ResClearArcadeBBQ response = new();

        int newscore = req.Score * 10; //10倍分数
        if (newscore > user.BBQInfoData.HighScore)
        {
            user.BBQInfoData.HighScore = newscore;

            if (user.Guild.guildId > 0)
            {
                MiniGameHelper.InsertOrUpdate(req.ArcadeId, user.ID, user.Guild.guildId.Value, newscore, 0);
            }

            user.AddTrigger(Trigger.EventBBQTycoonHighScore, newscore, 0);
        }

        user.BBQInfoData.PlayCount += 1;
        user.BBQInfoData.TotalAccumulatedScore += newscore;

        response.Data = user.BBQInfoData;

        JsonDb.Save();    
        // TODO
        await WriteDataAsync(response);
    }
}