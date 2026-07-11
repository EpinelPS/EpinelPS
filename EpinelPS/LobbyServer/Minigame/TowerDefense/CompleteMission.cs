using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/completemission")]
public class CompleteMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteArcadeTowerDefenseMission req = await ReadData<ReqCompleteArcadeTowerDefenseMission>();
        User user = GetUser();
        ResCompleteArcadeTowerDefenseMission response = new();

        NetRewardData ret = new NetRewardData();
        if (user.TowerDefenseDatas.TryGetValue(req.ArcadeManagerId, out TowerDefenseData? data))
        {
            List<int> rewardlist = [];
            foreach (var item in req.MissionUidList)
            {
                ArcadeTowerDefenseMissionProgress? commiss = data.MissionProgressList.FirstOrDefault(x => x.MissionUid == item);
                if (commiss!=null)
                {
                    commiss.ReceivedAt = DateTime.UtcNow.Date.ToTimestamp();
                    EventTowerDefenseMissionRecord? mission = GameData.Instance.EventTowerDefenseMissionTable.Values
                        .Where(x => x.Id == item).FirstOrDefault();
                    switch (mission.RewardType)
                    {                       
                        case EventTowerDefenseMissionRewardType.DailyPoint:
                            List<EventTowerDefenseMissionRecord>? dailylist = GameData.Instance.EventTowerDefenseMissionTable.Values
                                .Where(x => x.MissionCategory == EventTowerDefenseMissionCategory.DailyPoint).ToList();
                            foreach (var citem in dailylist)
                            {
                                var pro = data.MissionProgressList.FirstOrDefault(x => x.MissionUid == citem.Id);
                                if (pro?.ReceivedAt == null) pro.Progress += mission.RewardValue;                                
                            }
                            break;
                        case EventTowerDefenseMissionRewardType.Item:
                            rewardlist.Add(mission.RewardValue);
                            break;
                        default:
                            break;
                    }
                }
            }

            ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist);
            var missprolist = MiniGameHelper
                .ToProtoList<NetArcadeTowerDefenseMissionProgress, ArcadeTowerDefenseMissionProgress>(data.MissionProgressList);
            response.MissionProgressList.AddRange(missprolist);
            response.Reward = ret;
        }
        // TODO
        JsonDb.Save();

        // TODO
        await WriteDataAsync(response);
    }
}