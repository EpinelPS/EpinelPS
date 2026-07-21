using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.Dessertrush;

[GameRequest("/arcade/dessertrush/clear")]
public class Clear : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArcadeDessertRush req = await ReadData<ReqClearArcadeDessertRush>();
        User user = GetUser();
        ResClearArcadeDessertRush response = new();

        int newscore = req.Score;

        if (newscore > user.DessertRushData.HighScore)
        {
            user.DessertRushData.HighScore = newscore;
            if (user.Guild.guildId > 0)
            {
                MiniGameHelper.InsertOrUpdate(req.ArcadeId, user.ID, user.Guild.guildId.Value, newscore, 0);
            }            
        }        
        user.DessertRushData.TotalAccumulatedScore += newscore;
        response.Data =MiniGameHelper.ToProto<NetArcadeDessertRushData, ArcadeDessertRushData>( user.DessertRushData);
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}