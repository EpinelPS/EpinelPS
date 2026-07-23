using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeBubbleMarchData req = await ReadData<ReqGetArcadeBubbleMarchData>();
        User user = GetUser();
        ResGetArcadeBubbleMarchData response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        var manage = GameData.Instance.EventBubbleMarchManagerTable.Values
            .Where(x => x.Id == arcade.GameManagerId).FirstOrDefault();

        if (!user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId,out var marchData))
        {
            marchData = new();

            var missionlist = GameData.Instance.EventBubbleMarchMissionTable.Values
                .Where(x => x.MissionGroup == manage.MissionGroup).ToList();

            foreach (var item in missionlist)
            {
                marchData.AchievementMissionDataList.TryAdd(item.Id, new()
                {
                    IsReceived = false,
                    MissionId = item.Id,
                    Progress = 0
                });
            }

            user.BubbleMarchDatas[arcade.GameManagerId] = marchData;
        }

        response.AchievementMissionDataList.AddRange(MiniGameHelper
            .ToProtoDict<int, NetMiniGameBubbleMarchMissionData, MiniGameBubbleMarchMissionData>(marchData.AchievementMissionDataList).Values);
        response.BuffUpgradeIdList.AddRange(marchData.BuffUpgradeIdList);
        response.CharacterUpgradeIdList.AddRange(marchData.CharacterUpgradeIdList);
        response.ClearedStageIdList.AddRange(marchData.ClearedStageIdList);
        response.ClearedTutorialIdList.AddRange(marchData.ClearedTutorialIdList);
        response.LevelHideOptionActive = marchData.LevelHideOptionActive;

        foreach (var item in marchData.UpgradeCurrency)
        {
            response.UpgradeCurrency.TryAdd(item.Key, item.Value);
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}