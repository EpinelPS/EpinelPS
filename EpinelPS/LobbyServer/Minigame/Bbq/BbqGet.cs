using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.Bbq;

[GameRequest("/arcade/bbq/get")]
public class BbqGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeBBQData req = await ReadData<ReqGetArcadeBBQData>();
        User user = GetUser();
        ResGetArcadeBBQData response = new();

        if (user.BBQInfoData.ArcadeId == 0 )
        {
            user.BBQInfoData.ArcadeId = req.ArcadeId;
            
        }

        response.Data = user.BBQInfoData.ToNet();

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}