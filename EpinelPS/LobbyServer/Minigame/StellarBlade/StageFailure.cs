using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/stage/failure")]
public class StageFailure : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeFailureStellarBladeStage req = await ReadData<ReqArcadeFailureStellarBladeStage>();
        User user = GetUser();
        ResArcadeFailureStellarBladeStage response = new();
        NetStellarBladeRewardData ret = new();

        EventSBManagerRecord_Raw? sbm = GameData.Instance.EventSBManagerTable.Values
            .Where(s => s.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();
        EventSBStageRecord_Raw? stage = GameData.Instance.EventSBStageTable.Values
            .Where(s => s.Id == req.StageId).FirstOrDefault();


        int score = 0;

        score = req.DealtDamage * 4 - req.DamageTaken + 5000 * req.PerfectDodgeCount + 10000 * req.PerfectGuardCount - 1000 * req.PotionUsedCount;

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {

            if (stellar.StatisticsData.TryGetValue(req.StageId, out var netStatistics))
            {
                
                netStatistics.TotalPerfectDodgeCount += req.PerfectDodgeCount;
                netStatistics.TotalPerfectGuardCount += req.PerfectGuardCount;
                netStatistics.TotalDamageTaken += req.DamageTaken;
                netStatistics.TotalPotionUsedCount += req.PotionUsedCount;
                netStatistics.MinDuration = netStatistics.MinDuration.Min(req.Duration);
            }
            else
            {
                stellar.StatisticsData.TryAdd(req.StageId, new ResArcadeGetStellarBladeStatistics.Types.NetStatisticsData()
                {
                    StageId = req.StageId,
                    MinDuration = req.Duration,
                    TotalDamageTaken = req.DamageTaken,                    
                    TotalPerfectDodgeCount = req.PerfectDodgeCount,
                    TotalPerfectGuardCount = req.PerfectGuardCount,
                    TotalPotionUsedCount = req.PotionUsedCount
                });

               
            }

            List<SBMissionType> types = [SBMissionType.PerfectGuard,SBMissionType.Damage,
                SBMissionType.PerfectDodge,SBMissionType.PlayTime,SBMissionType.UsePotion];

            foreach (var type in types)
            {
                MiniGameHelper.UpMission(ref stellar, missionType: type, missionGroupId: sbm.MissionGroupId, stage: req.StageId, damage: req.DealtDamage, usePotion: req.PotionUsedCount,
                    PerfectGuard: req.PerfectGuardCount, PerfectDodge: req.PerfectDodgeCount, playTime: (int)req.Duration.Seconds);
            }

            MiniGameHelper.GetReward(ref ret, ref stellar, stage.StageFailureReward, score);
        }

        response.RewardData = ret;
        response.Score = score;

        JsonDb.Save();

        // TODO
        await WriteDataAsync(response);
    }
}