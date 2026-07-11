using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.Dessertrush;

[GameRequest("/arcade/dessertrush/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeDessertRushData req = await ReadData<ReqGetArcadeDessertRushData>();
        User user = GetUser();
        ResGetArcadeDessertRushData response = new();

        if (user.DessertRushData.ArcadeId == 0)
        {
            user.DessertRushData.ArcadeId = req.ArcadeId;
        }
        response.Data = MiniGameHelper.ToProto<NetArcadeDessertRushData, ArcadeDessertRushData>(user.DessertRushData);
        // TODO
        await WriteDataAsync(response);
    }
}