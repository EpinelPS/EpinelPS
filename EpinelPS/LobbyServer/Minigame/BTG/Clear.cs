using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BTG;

[GameRequest("/arcade/btg/clear")]
public class Clear : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArcadeBtg req = await ReadData<ReqClearArcadeBtg>();
        User user = GetUser();
        ResClearArcadeBtg response = new();
        
        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
            .Where(x => x.GameType == ArcadeGameType.BTG).FirstOrDefault();

        if (user.BtgData.TryGetValue(req.BtgId, out var btgData))
        {
            if (req.Score > btgData.Score)
            {
                btgData.Score = req.Score;

                if (user.Guild.guildId > 0)
                {
                    MiniGameHelper.InsertOrUpdate(arcade.Id, user.ID, user.Guild.guildId.Value, req.Score, req.BtgId);
                }
                
            }

            btgData.Data.CutSceneList.AddUnique(req.UnlockedCutScene);
            btgData.Data.WallpaperList.AddUnique(req.UnlockedWallpaper);

            btgData.Data.TotalAccumulatedScore += req.Score;

            UpMission(req.Score, req.ClearData, btgData);

            response.BtgData =MiniGameHelper.ToProto<NetArcadeBtgData,BtgData>( btgData.Data);
            var missionlist = MiniGameHelper.ToProtoDict<int, NetMiniGameBtgMissionData, MiniGameBtgMissionData>(btgData.MissionDatas);
            response.AchievementMissionDataList.AddRange(missionlist.Values);            
        }

        user.AddTrigger(Trigger.EventBFGCleanUpRewardCheck, 1, 1);

        response.BanResult = MiniGameBanResult.Success;
        
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }

    private void UpMission(int score,NetMiniGameBtgClearData clearData, ArcadeBtgData btgData)
    {
        if (btgData.MissionDatas.Count == 0) return;

        // 获取所有任务配置
        List<EventBTGMissionRecord_Raw>? missions = GameData.Instance.EventBTGMissionTable.Values
            .Where(x => x.ManagerId == btgData.Data.BtgId)
            .ToList();

        // 处理 GetPointStack
        foreach (EventBTGMissionRecord_Raw miss in missions.Where(x => x.ConditionType == EventBTGMissionConditionType.GetPointStack))
        {
            if (btgData.MissionDatas.TryGetValue(miss.Id, out var vale))
            {
                if (!vale.IsReceived)
                {
                    vale.Progress += score;
                }               
            }
        }

        // 处理 CumulativeSkillCount
        foreach (EventBTGMissionRecord_Raw miss in missions.Where(x => x.ConditionType == EventBTGMissionConditionType.UseSkillCount))
        {
            if (btgData.MissionDatas.TryGetValue(miss.Id, out var vale))
            {
                if (!vale.IsReceived)
                {
                    vale.Progress += miss.ConditionId switch
                    {
                        1 => clearData.UseSkill1Count,
                        2 => clearData.UseSkill2Count,
                        _ => 0
                    };
                }
            }
        }
    }
}