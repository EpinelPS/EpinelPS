using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PirateCafe;

[GameRequest("/arcade/PirateCafe/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadePirateCafeData req = await ReadData<ReqGetArcadePirateCafeData>();
        User user = GetUser();
        ResGetArcadePirateCafeData response = new();

        if (user.PirateCafeData.ArcadeId == 0)
        {
            user.PirateCafeData.ArcadeId = req.ArcadeId;
        }

        response.Data = MiniGameHelper.ToProto<NetArcadePirateCafeData, ArcadePirateCafeData>(user.PirateCafeData);
        // TODO
        await WriteDataAsync(response);
    }
}