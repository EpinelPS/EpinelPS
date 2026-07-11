using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/updatemissionprogress")]
public class UpdateMissionProgress : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUpdateArcadeTowerDefenseMissionProgress req = await ReadData<ReqUpdateArcadeTowerDefenseMissionProgress>();
        User user = GetUser();
        ResUpdateArcadeTowerDefenseMissionProgress response = new();

        if (user.TowerDefenseDatas.TryGetValue(req.ArcadeManagerId, out TowerDefenseData? data))
        {
            foreach (var item in req.ProgressUpdateList)
            {
                var pro = data.MissionProgressList.FirstOrDefault(x => x.MissionUid == item.MissionUid);
                if (pro?.ReceivedAt == null) pro.Progress += item.ProgressCount;
                
            }
            var missprolist = MiniGameHelper
                .ToProtoList<NetArcadeTowerDefenseMissionProgress, ArcadeTowerDefenseMissionProgress>(data.MissionProgressList);
            response.MissionProgressList.AddRange(missprolist);
        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}