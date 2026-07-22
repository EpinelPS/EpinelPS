using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using static EpinelPS.ResSuddenEventComplete.Types.BuffRewardData.Types;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/finishgeneralstage")]
public class FinishGeneralStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinishArcadeBubbleMarchGeneralStage req = await ReadData<ReqFinishArcadeBubbleMarchGeneralStage>();
        User user = GetUser();
        ResFinishArcadeBubbleMarchGeneralStage response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
                     .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {
            var stage = GameData.Instance.EventBubbleMarchStageTable.Values
                .Where(x => x.Id == req.StageId).FirstOrDefault();


            if (req.IsWin && !marchData.ClearedStageIdList.Contains(req.StageId))
            {
                marchData.ClearedStageIdList.Add(req.StageId);
                marchData.UpgradeCurrency[stage.FirstRewardCurrencyId] =
                    marchData.UpgradeCurrency.GetValueOrDefault(stage.FirstRewardCurrencyId) + stage.FirstRewardCurrencyValue;

                response.RewardUpgradeCurrency[stage.FirstRewardCurrencyId] = stage.FirstRewardCurrencyValue;
            }
           
            var progress = new BmHelper.MissionProgressData
            {
                CostCurrency = (1, req.StageMissionData.UseCurrency),
                ClearStage = req.StageId,
                HeroSummon = req.StageMissionData.HeroSummon.Keys.ToList(),
                KillEnemy = req.StageMissionData.KillEnemy.Values.Sum(),
                WaveClear = req.StageMissionData.WaveClear,
                MinionSummon =req.StageMissionData.MinionSummon.Values.Sum(),
                TargetLevelUp = req.StageMissionData.TargetLevelUp.Values.Max(),
                LevelUpCount = req.StageMissionData.LevelUpCount                
            };

            BmHelper.UpMission(marchData, MiniGameSystemType.Arcade, progress);           

            response.UpdatedAchievementMissions.AddRange(MiniGameHelper
                .ToProtoDict<int, NetMiniGameBubbleMarchMissionData, MiniGameBubbleMarchMissionData>(marchData.AchievementMissionDataList).Values);

            foreach (var item in marchData.UpgradeCurrency)
            {
                response.CurrentUpgradeCurrency.TryAdd(item.Key, item.Value);
            }
        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}