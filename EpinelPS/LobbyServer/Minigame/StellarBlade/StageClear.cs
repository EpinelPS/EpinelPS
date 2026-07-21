using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/stage/clear")]
public class StageClear : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeClearStellarBladeStage req = await ReadData<ReqArcadeClearStellarBladeStage>();
        User user = GetUser();
        ResArcadeClearStellarBladeStage response = new();
        NetStellarBladeRewardData ret = new()
        {
            Reward = new()
            {
                PassPoint = { }
            }
        };
        NetStellarBladeRewardData fret = new()
        {
            Reward = new()
            {
                PassPoint = { }
            }
        };


        EventSBStageRecord_Raw? stage = GameData.Instance.EventSBStageTable.Values
            .Where(s => s.Id == req.StageId).FirstOrDefault();
        EventSBManagerRecord_Raw? sbm = GameData.Instance.EventSBManagerTable.Values
            .Where(s=>s.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();
        
        int score = 0;

        score = req.DealtDamage * 4 - req.DamageTaken + 5000 * req.PerfectDodgeCount + 10000 * req.PerfectGuardCount - 1000 * req.PotionUsedCount;

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            
            if (stellar.StatisticsData.TryGetValue(req.StageId,out var netStatistics))
            {
                netStatistics.TotalKillCount += 1;
                netStatistics.TotalPerfectDodgeCount += req.PerfectDodgeCount;
                netStatistics.TotalPerfectGuardCount += req.PerfectGuardCount;
                netStatistics.TotalDamageTaken += req.DamageTaken;
                netStatistics.TotalPotionUsedCount += req.PotionUsedCount;
                netStatistics.MinDuration = netStatistics.MinDuration.Min(req.Duration);


                if (stellar.BestStageDatas.TryGetValue(req.StageId, out var sBStage))
                {
                    response.IsBestScore = score > sBStage.BestScore;
                    if (response.IsBestScore) sBStage.BestScore = score;

                    response.IsBestDealtDamage = req.DealtDamage > sBStage.BestDealtDamage;
                    if (response.IsBestDealtDamage) sBStage.BestDealtDamage = req.DealtDamage;

                    response.IsBestDuration = req.Duration.LessThan(sBStage.BestDuration);                                        
                    if (response.IsBestDuration)
                    {
                        sBStage.BestDuration = req.Duration;
                    }
                }
            }
            else
            {
                stellar.StatisticsData.TryAdd(req.StageId, new StatisticsData()
                {
                    StageId = req.StageId,
                    MinDuration = req.Duration,
                    TotalDamageTaken = req.DamageTaken,
                    TotalKillCount = 1,
                    TotalPerfectDodgeCount = req.PerfectDodgeCount,
                    TotalPerfectGuardCount = req.PerfectGuardCount,
                    TotalPotionUsedCount = req.PotionUsedCount
                });

               

                foreach (var item in stage.StageFirstClearReward)
                {
                    if (item.StageFirstClearReward > 0)
                    {
                        MiniGameHelper.GetReward(ref fret,ref stellar, item.StageFirstClearReward);
                    }
                }
                stellar.BestStageDatas.TryAdd(req.StageId, new() 
                { BestDealtDamage = req.DealtDamage, BestDuration = req.Duration, BestScore = score });




                response.IsBestDealtDamage = true;
                response.IsBestDuration = true;
                response.IsBestScore = true;

            }
            

            response.FirstClearReward = fret;
            
            
            MiniGameHelper.GetReward(ref ret, ref stellar, stage.StageClearReward);
            MiniGameHelper.GetReward(ref ret, ref stellar, stage.StageFailureReward, score);
            response.ClearReward = ret;

            List<SBMissionType> types = [SBMissionType.StageClear, SBMissionType.PerfectGuard,SBMissionType.Damage,
                SBMissionType.PerfectDodge,SBMissionType.PlayTime,SBMissionType.UsePotion];

            foreach (var type in types)
            {
                MiniGameHelper.UpMission(ref stellar, missionType: type, missionGroupId: sbm.MissionGroupId, stage: req.StageId, damage: req.DealtDamage, usePotion: req.PotionUsedCount,
                     PerfectGuard: req.PerfectGuardCount, PerfectDodge: req.PerfectDodgeCount, playTime: (int)req.Duration.Seconds);
            }

            MiniGameHelper.InsertOrUpdate(req.ArcadeManagerId, user.ID, user.Guild.guildId.Value, score,0);
        }

        response.Score = score;
        response.TruncatedDuration = req.Duration;

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }

   

    

}



