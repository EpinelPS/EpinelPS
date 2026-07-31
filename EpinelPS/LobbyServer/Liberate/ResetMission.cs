using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/resetmission")]
public class ResetMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqResetLiberateMission req = await ReadData<ReqResetLiberateMission>();
        User user = GetUser();
        ResResetLiberateMission response = new();

        if (user.LiberateDatas.TryGetValue(user.CurCharacterIdId, out var liberateData))
        {
            // 直接获取 groupId，如果为 null 则提前返回或处理
            NetLiberateMissionData? firstMission = liberateData.MissionData.FirstOrDefault(x => x.Id == req.MissionId);
            LiberateMissionRecord? currentMission = GameData.Instance.LiberateMissionTable.Values
                .FirstOrDefault(x => x.Id == firstMission.MissionTid);

            LiberateMissionGroupRecord? resetgroup = GameData.Instance.LiberateMissionGroupTable.Values
                .Where(x => x.DefaultMissionGroupId == currentMission.GroupId && x.MissionStep == liberateData.Step)
                .FirstOrDefault();
            List<int> missIds = new();

            if (resetgroup == null)
            {
                missIds = GameData.Instance.LiberateMissionTable.Values
                    .Where(x => x.GroupId == currentMission.GroupId)
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();
            }
            else
            {
                missIds = GameData.Instance.LiberateMissionTable.Values
                    .Where(x => x.GroupId == resetgroup.ResetMissionGroupId)
                    .Select(x => x.Id)
                    .Distinct()
                    .ToList();
            }
            var randomId = missIds[Random.Shared.Next(missIds.Count)]; // 使用 Random.Shared 避免每次 new

            firstMission.MissionTid = randomId;
            firstMission.MissionState = LiberateMissionState.Running;
            firstMission.CreatedAt = DateTime.UtcNow.Ticks;
            firstMission.ReceivedAt = DateTime.UtcNow.Ticks;
            firstMission.TriggerStartAt = DateTime.UtcNow.Ticks;
            firstMission.TriggerEndAt = DateTime.UtcNow.AddHours(24).Ticks;

            response.Data = liberateData;
            response.Error = LiberateDataExpiredError.Success;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}