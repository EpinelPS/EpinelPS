using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

public class BmHelper
{
    public class MissionProgressData
    {
        public int ClearStage { get; set; }
        public int TargetLevelUp { get; set; }
        public int LevelUpCount { get; set; }
        public int KillEnemy { get; set; }
        public List<int> HeroSummon { get; set; } = [];
        public int MinionSummon { get; set; }
        public int ChallengeWaveClear { get; set; }
        public int WaveClear { get; set; }
        public int DailyPoint { get; set; }
        public int BuffLevel { get; set; }
        public (int Id, int Count)? CostCurrency { get; set; }
    }


    public static void UpMission(BubbleMarchData marchData, MiniGameSystemType arcade, MissionProgressData progress)
    {
        if (progress == null) return;

        var manage = GameData.Instance.EventBubbleMarchManagerTable.Values
            .FirstOrDefault(x => x.MinigameType == arcade);
        if (manage == null) return;

        var missionDataMap = new Dictionary<EventBubbleMarchMissionConditionType, (int Progress, int ConditionId, bool IsIncrement)>
    {
        { EventBubbleMarchMissionConditionType.WaveClear, (progress.WaveClear, 0, true) },
        { EventBubbleMarchMissionConditionType.ChallengeWaveClear, (progress.ChallengeWaveClear, 0, true) },
        { EventBubbleMarchMissionConditionType.MinionSummon, (progress.MinionSummon, 0, true) },
        { EventBubbleMarchMissionConditionType.KillEnemy, (progress.KillEnemy, 0, true) },
        { EventBubbleMarchMissionConditionType.LevelUpCount, (progress.LevelUpCount, 0, true) },
        { EventBubbleMarchMissionConditionType.TargetLevelUp, (progress.TargetLevelUp, 0, false) },
        { EventBubbleMarchMissionConditionType.DailyPoint, (progress.DailyPoint, 0, false) },
        { EventBubbleMarchMissionConditionType.BuffLevel, (progress.BuffLevel, 0, false) },
        { EventBubbleMarchMissionConditionType.StageClear, (progress.ClearStage, progress.ClearStage, false) },
        { EventBubbleMarchMissionConditionType.UseCurrency, (progress.CostCurrency?.Count ?? 0, progress.CostCurrency?.Id ?? 0, true) }
    };

        var allMissions = GameData.Instance.EventBubbleMarchMissionTable.Values
            .Where(x => x.MissionGroup == manage.MissionGroup)
            .ToList();

        foreach (var kvp in missionDataMap)
        {
            var (progressValue, conditionId, isIncrement) = kvp.Value;
            if (progressValue <= 0) continue;

            var missions = allMissions
                .Where(x => x.ConditionType == kvp.Key && (conditionId == 0 || x.ConditionId == conditionId))
                .ToList();

            foreach (var mission in missions)
            {
                if (marchData.AchievementMissionDataList.TryGetValue(mission.Id, out var missionData))
                {
                    missionData.Progress = isIncrement ? missionData.Progress + progressValue : progressValue;
                }
            }
        }

        // 特殊处理 HeroSummon（列表）
        if (progress.HeroSummon != null && progress.HeroSummon.Count > 0)
        {
            foreach (var heroId in progress.HeroSummon)
            {
                var missions = allMissions
                    .Where(x => x.ConditionType == EventBubbleMarchMissionConditionType.HeroSummon &&
                                x.ConditionId == heroId)
                    .ToList();

                foreach (var mission in missions)
                {
                    if (marchData.AchievementMissionDataList.TryGetValue(mission.Id, out var missionData))
                    {
                        missionData.Progress += 1;
                    }
                }
            }
        }

        JsonDb.Save();
    }
}

