using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/mission/get")]
public class MissionGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeGetStellarBladeMissionData req = await ReadData<ReqArcadeGetStellarBladeMissionData>();
        User user = GetUser();
        ResArcadeGetStellarBladeMissionData response = new();

        MiniGameHelper.InitStellarBladeData(user, req.ArcadeManagerId);

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            var misslist = MiniGameHelper.ToProtoList<NetStellarBladeMissionData, StellarBladeMissionData>(stellar.MissionData);
            response.AchievementMissionDataList.AddRange(misslist);
        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}