using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.SoloraidMuseum;

internal static class SoloRaidMuseumHelper
{
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
        Logging.WriteLine($"[SoloRaidMuseum] data weeklyBuffGroup=" +
                          $"{response.WeeklyBuff.CurrentMuseumWeeklyBuffGroupTableId}, " +
                          $"start={response.WeeklyBuff.WeeklyBuffStartDate.ToDateTime():O}, " +
                          $"end={response.WeeklyBuff.WeeklyBuffEndDate.ToDateTime():O}, now={now:O}");
        return response;
    }

    public static ResGetSoloRaidMuseumGroupData GetGroupData(User user, int groupId)
    {
        ResGetSoloRaidMuseumGroupData response = new();
        var stages = GameData.Instance.MuseumStageTable.Values
            .Where(x => x.GroupId == groupId).OrderBy(x => x.Order).ToList();
        Logging.WriteLine($"[SoloRaidMuseum] groupdata group={groupId}, stages={stages.Count}, " +
                          $"characters={user.Characters.Count}, storedMuseumStages={user.SoloRaidMuseumData.Count}");

        foreach (var stage in stages)
        {
            var data = GetStage(user, stage.Id);
            var active = data.StageMode == SoloRaidMuseumStageMode.NoLimit ? data.NoLimit : data.Challenge;
            var teams = GetTeams(user, data).ToList();
            Logging.WriteLine($"[SoloRaidMuseum] groupdata stage={stage.Id}, mode={data.StageMode}, " +
                              $"storedTeams={data.Teams.Count}, responseTeams={DescribeTeams(user, teams)}, " +
                              $"inProgress={active.IsInProgress}, openTeams=[{string.Join(',', active.OpenTeams)}]");
            response.StageBattleDataList.Add(new NetSoloRaidMuseumStageBattleData
            {
                StageId = stage.Id,
                StageJoinCount = active.StageJoinCount,
                TotalDamage = active.TotalDamage,
                TotalStep = active.TotalStep,
                JoinData = new NetSoloRaidMuseumJoinData(),
                Teams = { teams },
            });
            // StageReadyPage.UpdateUI only wires the open button after it finds a
            // non-null UserRanking. An omitted message leaves the page visible but
            // makes the challenge button inert (no best-log/open request is sent).
            response.PastRankingList.Add(new NetSoloRaidMuseumStagePastRanking
            {
                StageId = stage.Id,
                UserRanking = new NetSoloRaidMuseumUserRankingData
                {
                    Ranking = 0,
                    Damage = active.TotalDamage,
                    CurrentUserData = LobbyHandler.CreateWholeUserDataFromDbUser(user),
                },
                TotalUserCount = 1,
            });
        }
        return response;
    }

    public static ResGetSoloRaidMuseumMission GetMissions(User user, int stageId)
    {
        var stage = GetStage(user, stageId);
        ResGetSoloRaidMuseumMission response = new();
        foreach (var mission in GameData.Instance.MuseumMissionTable.Values
                     .Where(x => x.StageId == stageId).OrderBy(x => x.Order))
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
        var requestedTeams = teams.Distinct().ToList();
        var availableTeams = GetTeams(user, stage).ToList();
        var missingTeams = requestedTeams
            .Where(number => availableTeams.All(team => team.TeamNumber != number)).ToList();
        Logging.WriteLine($"[SoloRaidMuseum] open stage={stageId}, noLimit={noLimit}, " +
                          $"requestedTeams=[{string.Join(',', requestedTeams)}], " +
                          $"availableTeams={DescribeTeams(user, availableTeams)}, missing=[{string.Join(',', missingTeams)}]");
        mode.IsInProgress = true;
        mode.OpenTeams = requestedTeams;
        JsonDb.Save();
        return mode;
    }

    public static void Enter(User user, int stageId, bool noLimit, int teamNumber)
    {
        var stage = GetStage(user, stageId);
        var mode = noLimit ? stage.NoLimit : stage.Challenge;
        var availableTeams = GetTeams(user, stage).ToList();
        var selectedTeam = availableTeams.FirstOrDefault(x => x.TeamNumber == teamNumber);
        Logging.WriteLine($"[SoloRaidMuseum] enter stage={stageId}, noLimit={noLimit}, team={teamNumber}, " +
                          $"isInProgress={mode.IsInProgress}, openTeams=[{string.Join(',', mode.OpenTeams)}], " +
                          $"selectedTeam={(selectedTeam is null ? "missing" : DescribeTeam(user, selectedTeam))}, " +
                          $"availableTeams={DescribeTeams(user, availableTeams)}");
    }

    public static void Close(User user, int stageId, bool noLimit)
    {
        var stage = GetStage(user, stageId);
        (noLimit ? stage.NoLimit : stage.Challenge).IsInProgress = false;
        JsonDb.Save();
    }

    public static (SoloRaidMuseumStageData stage, SoloRaidMuseumModeData mode) SetDamage(
        User user, int stageId, bool noLimit, NetSoloRaidMuseumBattleData? battle)
    {
        var stage = GetStage(user, stageId);
        var mode = noLimit ? stage.NoLimit : stage.Challenge;
        var damage = Math.Max(0, battle?.Damage ?? 0);
        mode.TotalDamage += damage;
        mode.StageJoinCount++;
        if (noLimit) mode.TotalStep++;
        mode.IsInProgress = false;
        mode.Logs.Add(new SoloRaidMuseumLogData { Damage = damage, TeamNumber = battle?.Team ?? 0 });
        if (!noLimit && damage > 0) stage.IsNoLimitUnlocked = true;
        JsonDb.Save();
        return (stage, mode);
    }

    public static IEnumerable<NetSoloRaidMuseumLog> GetLogs(User user, int stageId, bool noLimit)
    {
        var stage = GetStage(user, stageId);
        return (noLimit ? stage.NoLimit : stage.Challenge).Logs.Select(x => new NetSoloRaidMuseumLog
        {
            Damage = x.Damage,
            TeamNumber = x.TeamNumber,
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
            BestDamage = mode.TotalDamage,
            BestStep = mode.TotalStep,
            TargetMissionId = nextMission?.Id ?? 0,
            TargetMissionProgress = nextMission is null ? 0 : MissionProgress(mode, nextMission.ConditionType),
            TargetMissionConditionValue = nextMission?.ConditionValue ?? 0,
            IsInProgress = mode.IsInProgress,
        };
    }

    public static NetSoloRaidMuseumJoinData JoinData(SoloRaidMuseumModeData mode) => new();

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

    internal static string DescribeTeams(IEnumerable<NetTeamData> teams)
    {
        var descriptions = teams.Select(DescribeTeam).ToList();
        return descriptions.Count == 0 ? "none" : string.Join(";", descriptions);
    }

    private static string DescribeTeams(User user, IEnumerable<NetTeamData> teams)
    {
        var descriptions = teams.Select(x => DescribeTeam(user, x)).ToList();
        return descriptions.Count == 0 ? "none" : string.Join(";", descriptions);
    }

    private static string DescribeTeam(NetTeamData team)
    {
        var csns = team.Slots.Where(x => x.Value > 0).Select(x => x.Value).ToList();
        return $"team#{team.TeamNumber}(members={csns.Count},csn=[{string.Join(',', csns)}])";
    }

    private static string DescribeTeam(User user, NetTeamData team)
    {
        var slots = team.Slots.Where(x => x.Value > 0)
            .Select(x => $"{x.Value}:owned={user.GetCharacterBySerialNumber(x.Value) is not null}:type={x.ValueType}");
        return $"team#{team.TeamNumber}(members={team.Slots.Count(x => x.Value > 0)},slots=[{string.Join(',', slots)}])";
    }

    private static NetUserSoloRaidMuseumStageMode ToMode(SoloRaidMuseumStageData stage) => new()
    {
        StageId = stage.StageId,
        StageMode = stage.StageMode,
        IsNoLimitUnlocked = stage.IsNoLimitUnlocked,
    };

    private static long MissionProgress(SoloRaidMuseumModeData mode, MuseumMissionConditionType type) =>
        type == MuseumMissionConditionType.GetTotalStageStep ? mode.TotalStep : mode.TotalDamage;
}
