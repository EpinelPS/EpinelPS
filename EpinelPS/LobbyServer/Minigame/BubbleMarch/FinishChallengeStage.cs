using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/finishchallengestage")]
public class FinishChallengeStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinishArcadeBubbleMarchChallengeStage req = await ReadData<ReqFinishArcadeBubbleMarchChallengeStage>();
        User user = GetUser();
        ResFinishArcadeBubbleMarchChallengeStage response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
                     .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {

            var stage = GameData.Instance.EventBubbleMarchStageTable.Values
                .Where(x => x.Id == req.StageId).FirstOrDefault();

            if (!marchData.ClearedStageIdList.Contains(req.StageId))
            {
                marchData.ClearedStageIdList.Add(req.StageId);
            }

            if (marchData.WaveProgress.TryGetValue(req.StageMissionData.WaveClear, out var wprogress))
            {
                if (req.WaveProgress > wprogress)
                {
                    wprogress = req.WaveProgress;

                    if (user.Guild.guildId > 0)
                    {
                        MiniGameHelper.InsertOrUpdate(arcade.Id, user.ID, user.Guild.guildId.Value, (int)wprogress, req.StageMissionData.WaveClear);
                    }

                }
            }
            else
            {
                marchData.WaveProgress[req.StageMissionData.WaveClear] = req.WaveProgress;
            }

            var progress = new BmHelper.MissionProgressData
            {
                ChallengeWaveClear = req.StageMissionData.WaveClear,
                CostCurrency = (1, req.StageMissionData.UseCurrency),
                ClearStage = req.StageId,
                HeroSummon = req.StageMissionData.HeroSummon.Keys.ToList(),
                KillEnemy = req.StageMissionData.KillEnemy.Values.Sum(),
                MinionSummon = req.StageMissionData.MinionSummon.Values.Sum(),
                TargetLevelUp = req.StageMissionData.TargetLevelUp.Values.Max(),
                LevelUpCount = req.StageMissionData.LevelUpCount
            };

            BmHelper.UpMission(marchData, MiniGameSystemType.Arcade, progress);

            response.UpdatedAchievementMissions.AddRange(MiniGameHelper
                .ToProtoDict<int, NetMiniGameBubbleMarchMissionData, MiniGameBubbleMarchMissionData>(marchData.AchievementMissionDataList).Values);
        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}