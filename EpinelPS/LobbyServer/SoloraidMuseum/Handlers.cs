using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.SoloraidMuseum;

[GameRequest("/soloraidmuseum/get/data")]
public class GetData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetSoloRaidMuseumData>();
        await WriteDataAsync(SoloRaidMuseumHelper.GetData(GetUser()));
    }
}

[GameRequest("/soloraidmuseum/get/groupdata")]
public class GetGroupData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetSoloRaidMuseumGroupData>();
        Logging.WriteLine($"[SoloRaidMuseum] request groupdata group={req.GroupId}", LogType.Debug);
        await WriteDataAsync(SoloRaidMuseumHelper.GetGroupData(GetUser(), req.GroupId));
    }
}

[GameRequest("/soloraidmuseum/get/mission")]
public class GetMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetSoloRaidMuseumMission>();
        await WriteDataAsync(SoloRaidMuseumHelper.GetMissions(GetUser(), req.StageId));
    }
}

[GameRequest("/soloraidmuseum/set/stagemode")]
public class SetStageMode : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqSetSoloRaidMuseumStageMode>();
        SoloRaidMuseumHelper.SetMode(GetUser(), req.StageId, req.StageMode);
        await WriteDataAsync(new ResSetSoloRaidMuseumStageMode());
    }
}

[GameRequest("/soloraidmuseum/get/rankersquad")]
public class GetRankerSquad : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetSoloRaidMuseumRankerSquad>();
        await WriteDataAsync(new ResGetSoloRaidMuseumRankerSquad());
    }
}

[GameRequest("/soloraidmuseum/get/mypastsquad")]
public class GetMyPastSquad : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetSoloRaidMuseumMyPastSquad>();
        ResGetSoloRaidMuseumMyPastSquad response = new();
        response.SquadList.AddRange(SoloRaidMuseumHelper.GetLogs(GetUser(), req.StageId, false));
        await WriteDataAsync(response);
    }
}

[GameRequest("/soloraidmuseum/challenge/get/log")]
public class GetChallengeLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetSoloRaidMuseumChallengeLog>();
        ResGetSoloRaidMuseumChallengeLog response = new();
        response.LogList.AddRange(SoloRaidMuseumHelper.GetLogs(GetUser(), req.StageId, false, true));
        await WriteDataAsync(response);
    }
}

[GameRequest("/soloraidmuseum/challenge/get/bestlog")]
public class GetChallengeBestLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetSoloRaidMuseumChallengeBestLog>();
        ResGetSoloRaidMuseumChallengeBestLog response = new();
        response.LogList.AddRange(SoloRaidMuseumHelper.GetLogs(GetUser(), req.StageId, false));
        await WriteDataAsync(response);
    }
}

[GameRequest("/soloraidmuseum/nolimit/get/log")]
public class GetNoLimitLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetSoloRaidMuseumNoLimitLog>();
        ResGetSoloRaidMuseumNoLimitLog response = new();
        response.LogList.AddRange(SoloRaidMuseumHelper.GetLogs(GetUser(), req.StageId, true, true));
        await WriteDataAsync(response);
    }
}

[GameRequest("/soloraidmuseum/nolimit/get/bestlog")]
public class GetNoLimitBestLog : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetSoloRaidMuseumNoLimitBestLog>();
        ResGetSoloRaidMuseumNoLimitBestLog response = new();
        response.LogList.AddRange(SoloRaidMuseumHelper.GetLogs(GetUser(), req.StageId, true));
        await WriteDataAsync(response);
    }
}

[GameRequest("/soloraidmuseum/challenge/open")]
public class OpenChallenge : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqOpenSoloRaidMuseumChallenge>();
        Logging.WriteLine($"[SoloRaidMuseum] request challenge/open stage={req.StageId}, teams=[{string.Join(',', req.TeamList)}]", LogType.Info);
        var mode = SoloRaidMuseumHelper.Open(GetUser(), req.StageId, false, req.TeamList);
        await WriteDataAsync(new ResOpenSoloRaidMuseumChallenge
        {
            StageJoinCount = mode.StageJoinCount,
            TotalDamage = mode.TotalDamage,
            JoinData = SoloRaidMuseumHelper.JoinData(mode),
        });
    }
}

[GameRequest("/soloraidmuseum/nolimit/open")]
public class OpenNoLimit : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqOpenSoloRaidMuseumNoLimit>();
        Logging.WriteLine($"[SoloRaidMuseum] request nolimit/open stage={req.StageId}, teams=[{string.Join(',', req.TeamList)}]", LogType.Info);
        var mode = SoloRaidMuseumHelper.Open(GetUser(), req.StageId, true, req.TeamList);
        await WriteDataAsync(new ResOpenSoloRaidMuseumNoLimit
        {
            StageJoinCount = mode.StageJoinCount,
            TotalDamage = mode.TotalDamage,
            TotalStep = mode.TotalStep,
            JoinData = SoloRaidMuseumHelper.JoinData(mode),
        });
    }
}

[GameRequest("/soloraidmuseum/challenge/enter")]
public class EnterChallenge : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqEnterSoloRaidMuseumChallenge>();
        SoloRaidMuseumHelper.Enter(GetUser(), req.StageId, false, req.Team);
        await WriteDataAsync(new ResEnterSoloRaidMuseumChallenge());
    }
}

[GameRequest("/soloraidmuseum/nolimit/enter")]
public class EnterNoLimit : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqEnterSoloRaidMuseumNoLimit>();
        SoloRaidMuseumHelper.Enter(GetUser(), req.StageId, true, req.Team);
        await WriteDataAsync(new ResEnterSoloRaidMuseumNoLimit());
    }
}

[GameRequest("/soloraidmuseum/challenge/close")]
public class CloseChallenge : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqCloseSoloRaidMuseumChallenge>();
        Logging.WriteLine($"[SoloRaidMuseum] request challenge/close stage={req.StageId}", LogType.Debug);
        SoloRaidMuseumHelper.Close(GetUser(), req.StageId, false);
        await WriteDataAsync(new ResCloseSoloRaidMuseumChallenge());
    }
}

[GameRequest("/soloraidmuseum/nolimit/close")]
public class CloseNoLimit : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqCloseSoloRaidMuseumNoLimit>();
        Logging.WriteLine($"[SoloRaidMuseum] request nolimit/close stage={req.StageId}", LogType.Debug);
        SoloRaidMuseumHelper.Close(GetUser(), req.StageId, true);
        await WriteDataAsync(new ResCloseSoloRaidMuseumNoLimit());
    }
}

[GameRequest("/soloraidmuseum/challenge/set/damage")]
public class SetChallengeDamage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqSetSoloRaidMuseumChallengeDamage>();
        var (stage, mode) = SoloRaidMuseumHelper.SetDamage(GetUser(), req.StageId, false, req.BattleData, req.BattleResult);
        await WriteDataAsync(new ResSetSoloRaidMuseumChallengeDamage
        {
            StageJoinCount = mode.StageJoinCount,
            TotalDamage = mode.TotalDamage,
            JoinData = SoloRaidMuseumHelper.JoinData(mode),
            UpdatedStageInfo = SoloRaidMuseumHelper.ToInfo(stage, mode, false),
            IsNoLimitUnlocked = stage.IsNoLimitUnlocked,
            UnreceivedMission = SoloRaidMuseumHelper.HasUnreceivedMission(stage, false),
        });
    }
}

[GameRequest("/soloraidmuseum/nolimit/set/damage")]
public class SetNoLimitDamage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqSetSoloRaidMuseumNoLimitDamage>();
        var (stage, mode) = SoloRaidMuseumHelper.SetDamage(GetUser(), req.StageId, true, req.BattleData, req.BattleResult);
        await WriteDataAsync(new ResSetSoloRaidMuseumNoLimitDamage
        {
            StageJoinCount = mode.StageJoinCount,
            TotalDamage = mode.TotalDamage,
            TotalStep = mode.TotalStep,
            JoinData = SoloRaidMuseumHelper.JoinData(mode),
            UpdatedStageInfo = SoloRaidMuseumHelper.ToInfo(stage, mode, true),
            UnreceivedMission = SoloRaidMuseumHelper.HasUnreceivedMission(stage, true),
        });
    }
}

[GameRequest("/soloraidmuseum/challenge/acquire/mission")]
public class AcquireChallengeMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqAcquireSoloRaidMuseumChallengeMission>();
        var user = GetUser();
        ResAcquireSoloRaidMuseumChallengeMission response = new() { Reward = new NetRewardData() };
        var reward = response.Reward;
        Acquire(user, req.MissionIdList, false, response.MissionDataList, ref reward);
        response.Reward = reward;
        await WriteDataAsync(response);
    }

    internal static void Acquire(User user, IEnumerable<int> missionIds, bool noLimit,
        Google.Protobuf.Collections.RepeatedField<NetSoloRaidMuseumMissionData> result, ref NetRewardData reward)
    {
        // Claim requests can arrive concurrently when the client retries or the
        // button is tapped repeatedly. Keep validation, marking and reward
        // mutation atomic for this user so a mission can only pay out once.
        lock (user)
        {
            List<NetRewardData> rewards = [];
            foreach (var missionId in missionIds.Distinct())
            {
                if (!GameData.Instance.MuseumMissionTable.TryGetValue(missionId, out var mission))
                {
                    Logging.WriteLine($"[SoloRaidMuseum] mission claim ignored unknown id={missionId}", LogType.Warning);
                    continue;
                }

                var expectedMode = noLimit ? MuseumStageModeType.NoLimit : MuseumStageModeType.Challenge;
                if (mission.ModeType != expectedMode) continue;
                var stage = SoloRaidMuseumHelper.GetStage(user, mission.StageId);
                var received = noLimit ? stage.ReceivedNoLimitMissions : stage.ReceivedChallengeMissions;
                var mode = noLimit ? stage.NoLimit : stage.Challenge;
                var progress = SoloRaidMuseumHelper.MissionProgress(mode, mission.ConditionType);
                var target = SoloRaidMuseumHelper.MissionTargetValue(mission);
                if (received.Contains(missionId))
                {
                    Logging.WriteLine($"[SoloRaidMuseum] mission claim skipped already-received id={missionId}, noLimit={noLimit}", LogType.Info);
                    continue;
                }
                if (progress < target)
                {
                    Logging.WriteLine($"[SoloRaidMuseum] mission claim skipped incomplete id={missionId}, progress={progress}, target={target}", LogType.Info);
                    continue;
                }

                received.Add(missionId);
                result.Add(new NetSoloRaidMuseumMissionData { MissionId = missionId, Progress = progress, IsReceived = true });
                if (mission.RewardId > 0)
                    rewards.Add(RewardUtils.RegisterRewardsForUser(user, mission.RewardId));
                Logging.WriteLine($"[SoloRaidMuseum] mission claimed id={missionId}, noLimit={noLimit}, reward={mission.RewardId}", LogType.Info);
            }
            if (rewards.Count > 0) reward = NetUtils.MergeRewards(rewards, user);
            JsonDb.Save();
        }
    }
}

[GameRequest("/soloraidmuseum/nolimit/acquire/mission")]
public class AcquireNoLimitMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqAcquireSoloRaidMuseumNoLimitMission>();
        var user = GetUser();
        ResAcquireSoloRaidMuseumNoLimitMission response = new() { Reward = new NetRewardData() };
        var reward = response.Reward;
        AcquireChallengeMission.Acquire(user, req.MissionIdList, true, response.MissionDataList, ref reward);
        response.Reward = reward;
        var firstMission = req.MissionIdList
            .Select(x => GameData.Instance.MuseumMissionTable.GetValueOrDefault(x))
            .FirstOrDefault(x => x is not null);
        if (firstMission is not null)
        {
            var points = SoloRaidMuseumHelper.GetRankingPoints(user, firstMission.StageId);
            response.TotalRankingPoint = points.total;
            response.GroupRankingPoint = points.group;
        }
        await WriteDataAsync(response);
    }
}
