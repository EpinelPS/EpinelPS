using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.PirateCafe;

[GameRequest("/arcade/PirateCafe/clear")]
public class Clear : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArcadePirateCafe req = await ReadData<ReqClearArcadePirateCafe>();
        User user = GetUser();
        ResClearArcadePirateCafe response = new();
        int score = req.Score;
        if (score > user.PirateCafeData.HighScore)
        {
            user.PirateCafeData.HighScore = score;

            if (user.Guild.guildId > 0)
            {
                MiniGameHelper.InsertOrUpdate(req.ArcadeId, user.ID, user.Guild.guildId.Value, score, 0);
            }      
        }
        
        user.PirateCafeData.TotalAccumulatedScore += score;

        response.Data = MiniGameHelper.ToProto<NetArcadePirateCafeData, ArcadePirateCafeData>(user.PirateCafeData);
        
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}