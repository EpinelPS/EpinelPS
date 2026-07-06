using EpinelPS.Data;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeTowerDefenseData req = await ReadData<ReqGetArcadeTowerDefenseData>();
        User user = GetUser();
        ResGetArcadeTowerDefenseData response = new();

        // 获取或初始化
        if (!user.TowerDefenseDatas.TryGetValue(req.ArcadeManagerId, out TowerDefenseData? data))
        {
           data =  MiniGameHelper.InitTDData(req.ArcadeManagerId, user);
        }

        // 使用 data
        response.ChallengeMaxScore = data.ChallengeMaxScore;
        response.ClearedStageIdList.AddRange(data.ClearedStageIdList);
        response.ClearedTutorialIdList.AddRange(data.ClearedTutorialIdList);
        response.MissionProgressList.AddRange(data.MissionProgressList);
        response.UpgradeCurrency = data.UpgradeCurrency;
        response.UpgradeIdList.AddRange(data.UpgradeIdList);


        await WriteDataAsync(response);
    }

    
}
