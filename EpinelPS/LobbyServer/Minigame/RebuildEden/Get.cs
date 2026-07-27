using EpinelPS.Data;
using EpinelPS.Database;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeRebuildEdenData req = await ReadData<ReqGetArcadeRebuildEdenData>();
        User user = GetUser();
        ResGetArcadeRebuildEdenData response = new();


        EventRebuildEdenManagerRecord_Raw? manger = GameData.Instance.EventRebuildEdenManagerTable.Values
            .Where(x => x.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();

        if (user.RebuildEdenDatas.TryGetValue(manger.Id, out var rebuildEden))
        {
            var missdata = MiniGameHelper.ToProtoDict<int, NetRebuildEdenMissionData, RebuildEdenMissionData>(rebuildEden.MissionDatas);

            response.AchievementMissions.AddRange(missdata.Values);
            response.Data = MiniGameHelper.ToProto<NetRebuildEdenData, EdenData>(rebuildEden.Data);
            response.FirstEnteredAt = rebuildEden.FirstEnteredAt;
        }
        else
        {
            RebuildEdenData rebuild = new();
            rebuild.FirstEnteredAt = DateTime.UtcNow.Date.ToTimestamp();

            List<EventRebuildEdenMissionRecord_Raw>? missionlist = GameData.Instance.EventRebuildEdenMissionTable.Values
                .Where(x => x.EventGroup == manger.EventGroup).ToList();
            foreach (var item in missionlist)
            {
                RebuildEdenMissionData data = new()
                {
                    MissionId = item.Id,
                    IsReceived = false,
                    Progress = 0
                };

                rebuild.MissionDatas.TryAdd(item.Id, data);
            }

            user.RebuildEdenDatas.TryAdd(manger.Id, rebuild);

            var missdata = MiniGameHelper.ToProtoDict<int, NetRebuildEdenMissionData, RebuildEdenMissionData>(rebuild.MissionDatas);
            response.AchievementMissions.AddRange(missdata.Values);
            response.Data = MiniGameHelper.ToProto<NetRebuildEdenData, EdenData>(rebuild.Data);
            response.FirstEnteredAt = rebuild.FirstEnteredAt;
        }


        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}