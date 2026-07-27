using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/stage/complete")]
public class StageComplete : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteArcadeDragonDungeonRunStage req = await ReadData<ReqCompleteArcadeDragonDungeonRunStage>();
        User user = GetUser();
        ResCompleteArcadeDragonDungeonRunStage response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.DragonDungeonRun).FirstOrDefault();
        user.DDRDatas.HasWatchedTutorial = true;
        user.DDRDatas.LastPlayCharacter = req.CharacterId;
        user.DDRDatas.TotalDistance += req.RealDistance;
        user.DDRDatas.TotalGold += req.RealGold;
        user.DDRDatas.TotalSkillCount += req.UseSkill;
        user.DDRDatas.NewCharacters.Remove(req.CharacterId);
        if (req.RealDistance >= 150)
        {
            user.DDRDatas.IsPhase2Unlocked = true;
        }
        else if (req.RealDistance >= 80)
        {
            user.DDRDatas.IsPhase1Unlocked = true;
        }

        if (req.RealDistance > user.DDRDatas.Distance)
        {
            user.DDRDatas.Distance = req.RealDistance;
            response.IsNewRecord = true;
        }


        int score = (req.RealDistance + req.RealGold) * 100; //100倍分数

        if (score > user.DDRDatas.Point)
        {
            user.DDRDatas.Point = score;
            if (user.Guild.guildId > 0)
            {
                MiniGameHelper.InsertOrUpdate(arcade.Id, user.ID, user.Guild.guildId.Value, score, 0);
            }
        }

        MiniGameHelper.GetDungeonRankByScore(user.DDRDatas, score);

        if (user.DDRDatas.StatisticsDatas.TryGetValue(req.CharacterId, out var statisticsData))
        {
            statisticsData.CumulativeDistance += req.RealDistance;
            statisticsData.CumulativeGold += req.RealGold;
            statisticsData.PlayCount += 1;
            statisticsData.CumulativeSkillCount += req.UseSkill;

            if (score > statisticsData.Score)
            {
                statisticsData.Score = score;
                if (user.Guild.guildId > 0)
                {
                    MiniGameHelper.InsertOrUpdate(arcade.Id, user.ID, user.Guild.guildId.Value, score, req.CharacterId);
                }
            }

            statisticsData.DeadCountList[req.DeadCutScenePrintType] =
                statisticsData.DeadCountList.GetValueOrDefault(req.DeadCutScenePrintType) + 1;
        }
        else
        {
            DDRStatisticsData statistics = new()
            {
                Score = score,
                CumulativeDistance = req.RealDistance,
                CumulativeGold = req.RealGold,
                CumulativeSkillCount = req.UseSkill,
                PlayCount = 1
            };

            List<EventDragonDungeonRunCutSceneRecord_Raw>? deadlist = GameData.Instance.EventDragonDungeonRunCutSceneTable.Values
                .Where(x => x.CutscenePrintTiming == EventDragonDungeonRunCutScenePrintTiming.Dead).ToList();

            foreach (var item in deadlist)
            {
                statistics.DeadCountList.TryAdd(item.Id, 0);
            }
            if (req.DeadCutScenePrintType > 0)
            {
                statistics.DeadCountList[req.DeadCutScenePrintType] += 1;
            }




            user.DDRDatas.StatisticsDatas.TryAdd(req.CharacterId, statistics);

        }

        if (user.DDRDatas.CutSceneList.TryAdd(req.DeadCutScenePrintType, true))
        {
            user.DDRDatas.HasUnconfirmedAlbum = true;
        }

        UpMission(user.DDRDatas, MiniGameSystemType.Arcade);
        AddCharacterCheck(user.DDRDatas, MiniGameSystemType.Arcade);
        response.NewCutSceneList.AddRange(user.DDRDatas.CutSceneList.Where(x => x.Value == true)
            .Select(x => x.Key)
            .ToList());

        List<EventDragonDungeonRunScenarioRecord_Raw>? scelist = GameData.Instance.EventDragonDungeonRunScenarioTable.Values.ToList();

        //原本是按天数开放剧情，我改成按游戏次数开放剧情了
        //Originally, scenarios were unlocked by days. I've updated the logic to unlock them by the number of game plays instead.
        int numb = user.DDRDatas.StatisticsDatas.Values.Sum(x => x.PlayCount);
        if (numb > 0 && numb < scelist.Count)
        {
            response.NextScenarioGroupId = scelist[numb - 1].ScenarioDialogGroupId;
        }

        response.TotalPoint = score;
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }

    public static void UpMission(DragonDungeonRunData data, MiniGameSystemType arcade)
    {
        EventDragonDungeonRunManagerRecord_Raw? ddrmanager = GameData.Instance.EventDragonDungeonRunManagerTable.Values
               .Where(x => x.MinigameType == arcade).FirstOrDefault();
        List<EventDragonDungeonRunMissionRecord_Raw>? mlist = GameData.Instance.EventDragonDungeonRunMissionTable.Values
            .Where(x => x.GroupId == ddrmanager.MissionGroupId).ToList();

        foreach (EventDragonDungeonRunMissionType missionType in Enum.GetValues(typeof(EventDragonDungeonRunMissionType)))
        {
            switch (missionType)
            {
                case EventDragonDungeonRunMissionType.GainPoint:
                    List<EventDragonDungeonRunMissionRecord_Raw>? gp = mlist
                        .Where(x => x.MissionType == missionType).ToList();
                    foreach (var item in gp)
                    {
                        if (!data.RewardedMissionIdList.Contains(item.Id))
                        {
                            if (data.MissionDatas.TryGetValue(item.Id, out var vaule))
                            {
                                if (data.Point >= vaule.MissionTargetId)
                                {
                                    vaule.Progress += 1;
                                }
                            }
                        }
                    }
                    break;
                case EventDragonDungeonRunMissionType.GainGold:
                    List<EventDragonDungeonRunMissionRecord_Raw>? gg = mlist
                        .Where(x => x.MissionType == missionType).ToList();
                    foreach (var item in gg)
                    {
                        if (!data.RewardedMissionIdList.Contains(item.Id))
                        {
                            if (data.MissionDatas.TryGetValue(item.Id, out var vaule))
                            {
                                vaule.Progress = data.TotalGold;
                            }
                        }
                    }
                    break;
                case EventDragonDungeonRunMissionType.MoveDistance:
                    List<EventDragonDungeonRunMissionRecord_Raw>? md = mlist
                        .Where(x => x.MissionType == missionType).ToList();
                    foreach (var item in md)
                    {
                        if (!data.RewardedMissionIdList.Contains(item.Id))
                        {
                            if (data.MissionDatas.TryGetValue(item.Id, out var vaule))
                            {
                                vaule.Progress = data.TotalDistance;
                            }
                        }
                    }
                    break;
                case EventDragonDungeonRunMissionType.UseSkillCount:
                    List<EventDragonDungeonRunMissionRecord_Raw>? usc = mlist
                       .Where(x => x.MissionType == missionType).ToList();
                    foreach (var item in usc)
                    {
                        if (!data.RewardedMissionIdList.Contains(item.Id))
                        {
                            if (data.MissionDatas.TryGetValue(item.Id, out var vaule))
                            {
                                vaule.Progress = data.TotalSkillCount;
                            }
                        }
                    }
                    break;
                case EventDragonDungeonRunMissionType.PlayCharacterCount:
                    List<EventDragonDungeonRunMissionRecord_Raw>? pcc = mlist
                       .Where(x => x.MissionType == missionType).ToList();
                    foreach (var item in pcc)
                    {
                        if (!data.RewardedMissionIdList.Contains(item.Id))
                        {
                            if (data.MissionDatas.TryGetValue(item.Id, out var vaule))
                            {
                                if (data.StatisticsDatas.TryGetValue(item.MissionTargetId, out var statistics))
                                {
                                    vaule.Progress = statistics.PlayCount;
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        JsonDb.Save();
    }

    public static void AddCharacterCheck(DragonDungeonRunData data, MiniGameSystemType arcade)
    {
        EventDragonDungeonRunManagerRecord_Raw? ddrmanager = GameData.Instance.EventDragonDungeonRunManagerTable.Values
               .Where(x => x.MinigameType == arcade).FirstOrDefault();
        foreach (EventDragonDungeonRunCharacterUnlockType unlockType in Enum.GetValues(typeof(EventDragonDungeonRunCharacterUnlockType)))
        {
            switch (unlockType)
            {
                case EventDragonDungeonRunCharacterUnlockType.GainPoint:
                    EventDragonDungeonRunCharacterRecord_Raw? chardata = GameData.Instance.EventDragonDungeonRunCharacterTable.Values
                        .Where(x => x.GroupId == ddrmanager.UseCharacterGroupId && x.OpenCondition == unlockType)
                        .FirstOrDefault();
                    if (chardata != null)
                    {
                        if (data.Point >= chardata.OpenConditionValue)
                        {
                            data.NewCharacters.AddUnique(chardata.Id);
                            data.Characters.AddUnique(chardata.Id);
                        }
                    }

                    break;
                case EventDragonDungeonRunCharacterUnlockType.GainGold:
                    EventDragonDungeonRunCharacterRecord_Raw? gchardata = GameData.Instance.EventDragonDungeonRunCharacterTable.Values
                        .Where(x => x.GroupId == ddrmanager.UseCharacterGroupId && x.OpenCondition == unlockType)
                        .FirstOrDefault();
                    if (gchardata != null)
                    {
                        if (data.TotalGold >= gchardata.OpenConditionValue)
                        {
                            data.NewCharacters.AddUnique(gchardata.Id);
                            data.Characters.AddUnique(gchardata.Id);
                        }
                    }
                    break;
                case EventDragonDungeonRunCharacterUnlockType.MoveDistance:
                    EventDragonDungeonRunCharacterRecord_Raw? mchardata = GameData.Instance.EventDragonDungeonRunCharacterTable.Values
                        .Where(x => x.GroupId == ddrmanager.UseCharacterGroupId && x.OpenCondition == unlockType)
                        .FirstOrDefault();
                    if (mchardata != null)
                    {
                        if (data.TotalDistance >= mchardata.OpenConditionValue)
                        {
                            data.NewCharacters.AddUnique(mchardata.Id);
                            data.Characters.AddUnique(mchardata.Id);
                        }
                    }
                    break;
                default:
                    break;
            }
        }


        JsonDb.Save();
    }
}