using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/get")]
public class GetLiberateData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetLiberateData req = await ReadData<ReqGetLiberateData>();
        User user = GetUser();
        Random random = new Random();
        ResGetLiberateData response = new();

        if (user.LiberateDatas.TryGetValue(user.CurCharacterIdId, out var liberateData))
        {
            if (liberateData.MissionData[0].CreatedAt.IsOlderThan24Hours())
            {
                // 直接获取 groupId，如果为 null 则提前返回或处理
                NetLiberateMissionData? firstMission = liberateData.MissionData[0];
                LiberateMissionRecord? currentMission = GameData.Instance.LiberateMissionTable.Values
                    .FirstOrDefault(x => x.Id == firstMission.MissionTid);

                if (currentMission == null) return; // 或其它错误处理

                // 一条链式查询完成：获取 GroupId 对应下的所有不重复 SubGroupId
                List<int>? subGroupIds = GameData.Instance.LiberateMissionTable.Values
                    .Where(x => x.GroupId == currentMission.GroupId)
                    .Select(x => x.SubGroupId)
                    .Distinct()
                    .ToList();

                var randomSubGroupId = subGroupIds[Random.Shared.Next(subGroupIds.Count)]; // 使用 Random.Shared 避免每次 new
                List<NetLiberateMissionData>? list = LiberateHelper.GetMissions(user.CurCharacterIdId, currentMission.GroupId, randomSubGroupId);

                liberateData.MissionData.Clear();
                liberateData.MissionData.AddRange(list);
                liberateData.RewardedCount = 0;
            }
            response.LiberateData = user.LiberateDatas.GetValueOrDefault(user.CurCharacterIdId);
            response.OpenLiberateTypeIdList.AddRange(user.OpenLiberateTypeIdList);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}