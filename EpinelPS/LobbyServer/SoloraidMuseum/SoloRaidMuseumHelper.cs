using EpinelPS.Data;
using EpinelPS.Database;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.SoloraidMuseum;

internal static class SoloRaidMuseumHelper
{
    private const int MaxTeamCount = 5;

    public static SoloRaidMuseumStageData GetStage(User user, int stageId)
    {
        if (!user.SoloRaidMuseumData.TryGetValue(stageId, out var data))
        {
            data = new SoloRaidMuseumStageData { StageId = stageId };
            user.SoloRaidMuseumData[stageId] = data;
        }

        return data;
    }

    public static ResGetSoloRaidMuseumData GetData(User user)
    {
        ResGetSoloRaidMuseumData response = new();
        var groups = GameData.Instance.MuseumGroupTable.Values.OrderBy(x => x.Order).ToList();
        foreach (var group in groups)
        {
            NetUserSoloRaidMuseumGroupInfo info = new() { GroupId = group.Id };
            foreach (var stage in GameData.Instance.MuseumStageTable.Values
                         .Where(x => x.GroupId == group.Id).OrderBy(x => x.Order))
            {
                var data = GetStage(user, stage.Id);
                info.StageModeList.Add(ToMode(data));
                info.ChallengeStageInfoList.Add(ToInfo(data, data.Challenge, false));
                info.NoLimitStageInfoList.Add(ToInfo(data, data.NoLimit, true));
            }
            response.GroupInfoList.Add(info);
        }

        var now = DateTime.UtcNow;
        var weekStart = now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7));
        response.WeeklyBuff = new NetSoloRaidMuseumWeeklyBuff
        {
            CurrentMuseumWeeklyBuffGroupTableId = GameData.Instance.MuseumWeeklyBuffGroupTable.Keys.DefaultIfEmpty().Max(),
            WeeklyBuffStartDate = Timestamp.FromDateTime(DateTime.SpecifyKind(weekStart, DateTimeKind.Utc)),
            WeeklyBuffEndDate = Timestamp.FromDateTime(DateTime.SpecifyKind(weekStart.AddDays(7), DateTimeKind.Utc)),
            WeeklyBuffSeasonEndDate = Timestamp.FromDateTime(DateTime.SpecifyKind(weekStart.AddDays(7), DateTimeKind.Utc)),
        };
        return response;
    }

    public static ResGetSoloRaidMuseumGroupData GetGroupData(User user, int groupId)
    {
        ResGetSoloRaidMuseumGroupData response = new();
        var stages = GameData.Instance.MuseumStageTable.Values
            .Where(x => x.GroupId == groupId).OrderBy(x => x.Order).ToList();
        foreach (var stage in stages)
        {
            var data = GetStage(user, stage.Id);
            var active = data.StageMode == SoloRaidMuseumStageMode.NoLimit ? data.NoLimit : data.Challenge;
            var teams = GetTeams(user, data).ToList();
            response.StageBattleDataList.Add(new NetSoloRaidMuseumStageBattleData
            {
                StageId = stage.Id,
                // The client also uses these values after a completed run to
                // open the result/battle flow.
                StageJoinCount = active.StageJoinCount,
                TotalDamage = active.TotalDamage,
                TotalStep = active.TotalStep,
                JoinData = active.IsInProgress ? JoinData(active) : new NetSoloRaidMuseumJoinData(),
                Teams = { teams },
            });
            // StageReadyPage.UpdateUI only wires the open button after it finds a
            // non-null UserRanking. An omitted message leaves the page visible but
            // makes the challenge button inert (no best-log/open request is sent).
            NetSoloRaidMuseumStagePastRanking pastRanking = new()
            {
                StageId = stage.Id,
                UserRanking = new NetSoloRaidMuseumUserRankingData
                {
                    Ranking = 0,
                    Damage = active.TotalDamage,
                    CurrentUserData = LobbyHandler.CreateWholeUserDataFromDbUser(user),
                },
                TotalUserCount = 1,
            };
            response.PastRankingList.Add(pastRanking);
        }
        return response;
    }

    public static ResGetSoloRaidMuseumMission GetMissions(User user, int stageId)
    {
        var stage = GetStage(user, stageId);
        ResGetSoloRaidMuseumMission response = new();
        var missions = GameData.Instance.MuseumMissionTable.Values
            .Where(x => x.StageId == stageId).OrderBy(x => x.Order).ToList();
        foreach (var mission in missions)
        {
            var mode = mission.ModeType == MuseumStageModeType.NoLimit ? stage.NoLimit : stage.Challenge;
            var received = mission.ModeType == MuseumStageModeType.NoLimit
                ? stage.ReceivedNoLimitMissions.Contains(mission.Id)
                : stage.ReceivedChallengeMissions.Contains(mission.Id);
            var net = new NetSoloRaidMuseumMissionData
            {
                MissionId = mission.Id,
                Progress = MissionProgress(mode, mission.ConditionType),
                IsReceived = received,
            };
            if (mission.ModeType == MuseumStageModeType.NoLimit)
                response.NoLimitMissionDataList.Add(net);
            else
                response.ChallengeMissionDataList.Add(net);
        }
        var points = GetRankingPoints(user, stageId);
        response.MuseumTotalRankingPoint = points.total;
        response.MuseumGroupRankingPoint = points.group;
        return response;
    }

    public static (int total, int group) GetRankingPoints(User user, int stageId)
    {
        var groupId = GameData.Instance.MuseumStageTable.TryGetValue(stageId, out var selectedStage)
            ? selectedStage.GroupId : 0;
        var total = 0;
        var group = 0;
        foreach (var stage in user.SoloRaidMuseumData.Values)
        {
            foreach (var missionId in stage.ReceivedChallengeMissions.Concat(stage.ReceivedNoLimitMissions))
            {
                if (!GameData.Instance.MuseumMissionTable.TryGetValue(missionId, out var mission)) continue;
                total += mission.RankingPointAmount;
                if (GameData.Instance.MuseumStageTable.TryGetValue(mission.StageId, out var missionStage) &&
                    missionStage.GroupId == groupId)
                    group += mission.RankingPointAmount;
            }
        }
        return (total, group);
    }

    public static NetUserSoloRaidMuseumStageMode SetMode(User user, int stageId, SoloRaidMuseumStageMode mode)
    {
        var stage = GetStage(user, stageId);
        if (mode == SoloRaidMuseumStageMode.NoLimit && !stage.IsNoLimitUnlocked)
            mode = SoloRaidMuseumStageMode.Challenge;
        stage.StageMode = mode;
        JsonDb.Save();
        return ToMode(stage);
    }

    public static SoloRaidMuseumModeData Open(User user, int stageId, bool noLimit, IEnumerable<int> teams)
    {
        var stage = GetStage(user, stageId);
        var mode = noLimit ? stage.NoLimit : stage.Challenge;
        var requestedTeams = teams.Distinct().Take(MaxTeamCount - 1).ToList();
        mode.CurrentLogs = mode.Logs
            .Where(x => requestedTeams.Contains(x.TeamNumber))
            .Select(x =>
            {
                var log = CloneLog(x);
                if (log.Team.Count == 0)
                    log.Team = CreateTeamSnapshot(user, stage, log.TeamNumber);
                log.Step = noLimit ? GetNoLimitStep(stageId, log.Damage) : 0;
                return log;
            })
            .ToList();
        mode.StageJoinCount = mode.CurrentLogs.Count;
        mode.TotalDamage = mode.CurrentLogs.Sum(x => x.Damage);
        mode.TotalStep = mode.CurrentLogs.Sum(x => x.Step);
        mode.IsInProgress = true;
        mode.OpenTeams = requestedTeams;
        JsonDb.Save();
        return mode;
    }

    public static void Enter(User user, int stageId, bool noLimit, int teamNumber)
    {
        var stage = GetStage(user, stageId);
        var mode = noLimit ? stage.NoLimit : stage.Challenge;
    }

    public static void Close(User user, int stageId, bool noLimit)
    {
        var stage = GetStage(user, stageId);
        var mode = noLimit ? stage.NoLimit : stage.Challenge;
        mode.IsInProgress = false;
        mode.OpenTeams.Clear();
        mode.CurrentLogs.Clear();
        mode.StageJoinCount = 0;
        mode.TotalDamage = 0;
        mode.TotalStep = 0;
        JsonDb.Save();
    }

    public static (SoloRaidMuseumStageData stage, SoloRaidMuseumModeData mode) SetDamage(
        User user, int stageId, bool noLimit, NetSoloRaidMuseumBattleData? battle)
    {
        var stage = GetStage(user, stageId);
        var mode = noLimit ? stage.NoLimit : stage.Challenge;
        var damage = Math.Max(0, battle?.Damage ?? 0);
        if (!mode.IsInProgress)
        {
            mode.IsInProgress = true;
            mode.CurrentLogs.Clear();
            mode.OpenTeams.Clear();
        }

        var teamNumber = battle?.Team ?? 0;
        mode.CurrentLogs.RemoveAll(x => x.TeamNumber == teamNumber);
        mode.CurrentLogs.Add(new SoloRaidMuseumLogData
        {
            Damage = damage,
            Step = noLimit ? GetNoLimitStep(stageId, damage) : 0,
            TeamNumber = teamNumber,
            Team = CreateTeamSnapshot(user, stage, teamNumber),
        });
        mode.StageJoinCount = mode.CurrentLogs.Count;
        mode.TotalDamage = mode.CurrentLogs.Sum(x => x.Damage);
        mode.TotalStep = mode.CurrentLogs.Sum(x => x.Step);

        if (mode.StageJoinCount >= MaxTeamCount)
        {
            mode.IsInProgress = false;
            // NoLimit records are ordered by total step first. Damage only breaks
            // ties, matching the result screen's best-step/best-damage comparison.
            var isNewRecord = noLimit
                ? mode.TotalStep > mode.BestStep ||
                  mode.TotalStep == mode.BestStep && mode.TotalDamage > mode.BestDamage
                : mode.TotalDamage > mode.BestDamage;
            if (isNewRecord)
            {
                if (noLimit)
                {
                    mode.BestStep = mode.TotalStep;
                    mode.BestDamage = mode.TotalDamage;
                }
                else
                {
                    mode.BestDamage = mode.TotalDamage;
                }
                mode.Logs = mode.CurrentLogs.Select(CloneLog).ToList();
            }

            if (!noLimit)
                stage.IsNoLimitUnlocked = HasClearedChallenge(stage.StageId, mode.BestDamage);
        }
        JsonDb.Save();
        return (stage, mode);
    }

    public static IEnumerable<NetSoloRaidMuseumLog> GetLogs(User user, int stageId, bool noLimit, bool current = false)
    {
        var stage = GetStage(user, stageId);
        var mode = noLimit ? stage.NoLimit : stage.Challenge;
        // The normal log endpoint represents the current/most recently completed
        // run. Keep it available for the result screen after the fifth team.
        var logs = current && mode.CurrentLogs.Count > 0 ? mode.CurrentLogs : mode.Logs;
        return logs.Select(x =>
        {
            var snapshot = x.Team.Count > 0 ? x.Team : CreateTeamSnapshot(user, stage, x.TeamNumber);
            return new NetSoloRaidMuseumLog
            {
                Damage = x.Damage,
                TeamNumber = x.TeamNumber,
                Team = { snapshot.Select(ToMuseumTeamCharacter) },
            };
        });
    }

    public static NetUserSoloRaidMuseumStageInfo ToInfo(
        SoloRaidMuseumStageData stage, SoloRaidMuseumModeData mode, bool noLimit)
    {
        var nextMission = GameData.Instance.MuseumMissionTable.Values
            .Where(x => x.StageId == stage.StageId &&
                        x.ModeType == (noLimit ? MuseumStageModeType.NoLimit : MuseumStageModeType.Challenge))
            .OrderBy(x => x.Order)
            .FirstOrDefault(x => !(noLimit ? stage.ReceivedNoLimitMissions : stage.ReceivedChallengeMissions).Contains(x.Id));
        return new NetUserSoloRaidMuseumStageInfo
        {
            StageId = stage.StageId,
            BestDamage = mode.BestDamage,
            BestStep = mode.BestStep,
            TargetMissionId = nextMission?.Id ?? 0,
            TargetMissionProgress = nextMission is null ? 0 : MissionProgress(mode, nextMission.ConditionType),
            TargetMissionConditionValue = nextMission is null ? 0 : MissionTargetValue(nextMission),
            IsInProgress = mode.IsInProgress,
        };
    }

    public static NetSoloRaidMuseumJoinData JoinData(SoloRaidMuseumModeData mode)
    {
        NetSoloRaidMuseumJoinData result = new();
        result.CsnList.AddRange(mode.CurrentLogs.SelectMany(x => x.Team).Select(x => x.Csn).Where(x => x > 0));
        return result;
    }

    private static IEnumerable<NetTeamData> GetTeams(User user, SoloRaidMuseumStageData stage)
    {
        if (stage.Teams.Any(x => x.Slots.Any(slot => slot.Value > 0)))
            return stage.Teams.Select(x => x.Clone()).ToList();

        // Compatibility with data written before museum teams became stage-scoped.
        var museumTeamType = (int)TeamType.SoloRaidMuseum;
        if (user.UserTeams.TryGetValue(museumTeamType, out var museumTeams) &&
            museumTeams.Teams.Any(x => x.Slots.Any(slot => slot.Value > 0)))
            return museumTeams.Teams.Select(x => x.Clone()).ToList();

        // Only account-owned characters are safe here. Other content presets may
        // contain temporary/support CSNs which the museum client cannot resolve.
        NetTeamData fallback = new() { TeamNumber = 1 };
        var characters = user.Characters.Take(5).ToList();
        for (var i = 0; i < characters.Count; i++)
            // NetTeamSlot.ValueType == 1 means a temporary support character.
            // Account-owned character serial numbers must use the default value 0.
            fallback.Slots.Add(new NetTeamSlot { Slot = i + 1, Value = characters[i].Csn, ValueType = 0 });
        return fallback.Slots.Count > 0 ? [fallback] : [];
    }

    private static bool HasClearedChallenge(int stageId, long bestDamage)
    {
        var noLimitMode = GameData.Instance.MuseumStageModeTable.Values.FirstOrDefault(x =>
            x.StageId == stageId && x.ModeType == MuseumStageModeType.NoLimit);
        var finalMission = noLimitMode is null
            ? null
            : GameData.Instance.MuseumMissionTable.GetValueOrDefault(noLimitMode.ModeOpenCondition);
        return finalMission is not null && bestDamage >= MissionTargetValue(finalMission);
    }

    private static int GetNoLimitStep(int stageId, long damage)
    {
        var mode = GameData.Instance.MuseumStageModeTable.Values.FirstOrDefault(x =>
            x.StageId == stageId && x.ModeType == MuseumStageModeType.NoLimit);
        if (mode is null) return 1;

        // MonsterStageLvChangeTable.GetStep on the client returns step 1 for zero
        // damage, otherwise the row whose damage interval contains this team's
        // damage. The final row has max=0 and is an open-ended interval.
        return GameData.Instance.MonsterStageLvChangeTable.Values
            .Where(x => x.Group == mode.MonsterStageLvChangeGroup)
            .OrderBy(x => x.ConditionValueMin)
            .LastOrDefault(x => damage >= x.ConditionValueMin)?.Step ?? 1;
    }

    public static bool HasUnreceivedMission(SoloRaidMuseumStageData stage, bool noLimit)
    {
        var mode = noLimit ? stage.NoLimit : stage.Challenge;
        var received = noLimit ? stage.ReceivedNoLimitMissions : stage.ReceivedChallengeMissions;
        var modeType = noLimit ? MuseumStageModeType.NoLimit : MuseumStageModeType.Challenge;
        return GameData.Instance.MuseumMissionTable.Values.Any(x =>
            x.StageId == stage.StageId && x.ModeType == modeType && !received.Contains(x.Id) &&
            MissionProgress(mode, x.ConditionType) >= MissionTargetValue(x));
    }

    public static long MissionTargetValue(MuseumMissionRecord_Raw mission)
    {
        if (mission.ConditionType != MuseumMissionConditionType.GetTotalDamageRanker)
            return mission.ConditionValue;

        var rankMissions = GameData.Instance.MuseumMissionTable.Values
            .Where(x => x.StageId == mission.StageId &&
                        x.ModeType == MuseumStageModeType.Challenge &&
                        x.ConditionType == MuseumMissionConditionType.GetTotalDamageRanker)
            .OrderBy(x => x.Order)
            .ToList();
        var index = rankMissions.FindIndex(x => x.Id == mission.Id);
        if (index < 0) return long.MaxValue;
        return GameData.Instance.MuseumMissionTable.Values
            .Where(x => x.StageId == mission.StageId &&
                        x.ModeType == MuseumStageModeType.Challenge &&
                        x.ConditionType == MuseumMissionConditionType.GetTotalDamage)
            .OrderBy(x => x.ConditionValue)
            .TakeLast(rankMissions.Count)
            .Select(x => x.ConditionValue)
            .ElementAtOrDefault(index);
    }

    private static List<TeamCharacterData> CreateTeamSnapshot(User user, SoloRaidMuseumStageData stage, int teamNumber)
    {
        var team = GetTeams(user, stage).FirstOrDefault(x => x.TeamNumber == teamNumber);
        if (team is null) return [];
        return team.Slots.Where(x => x.Value > 0).Select(slot =>
        {
            var character = user.Characters.FirstOrDefault(x => x.Csn == slot.Value);
            return new TeamCharacterData
            {
                Slot = slot.Slot,
                Csn = slot.Value,
                Tid = character?.Tid ?? (int)slot.Value,
                Lv = character?.Level ?? 1,
                Combat = 0,
                CostumeId = character?.CostumeId ?? 0,
            };
        }).ToList();
    }

    private static NetSoloRaidMuseumTeamCharacter ToMuseumTeamCharacter(TeamCharacterData character) => new()
    {
        Slot = character.Slot,
        Tid = character.Tid,
        Lv = character.Lv,
        Combat = character.Combat,
        CostumeId = character.CostumeId,
    };

    private static SoloRaidMuseumLogData CloneLog(SoloRaidMuseumLogData log) => new()
    {
        Damage = log.Damage,
        Step = log.Step,
        TeamNumber = log.TeamNumber,
        Team = log.Team.Select(x => new TeamCharacterData
        {
            Slot = x.Slot,
            Csn = x.Csn,
            Tid = x.Tid,
            Lv = x.Lv,
            Combat = x.Combat,
            CostumeId = x.CostumeId,
        }).ToList(),
    };

    private static NetUserSoloRaidMuseumStageMode ToMode(SoloRaidMuseumStageData stage) => new()
    {
        StageId = stage.StageId,
        StageMode = stage.StageMode,
        IsNoLimitUnlocked = stage.IsNoLimitUnlocked,
    };

    public static long MissionProgress(SoloRaidMuseumModeData mode, MuseumMissionConditionType type) =>
        type == MuseumMissionConditionType.GetTotalStageStep ? mode.BestStep : mode.BestDamage;
}
