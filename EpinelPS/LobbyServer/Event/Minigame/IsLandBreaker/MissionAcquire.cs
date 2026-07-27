using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Minigame;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/mission/acquire")]
public class MissionAcquire : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAcquireMiniGameIslandBreakerMissionReward req = await ReadData<ReqAcquireMiniGameIslandBreakerMissionReward>();
        User user = GetUser();
        ResAcquireMiniGameIslandBreakerMissionReward response = new();
        NetRewardData ret = new();
        List<int> rewardlist = [];

        if (user.IsLandBreakerDatas.TryGetValue(req.IslandBreakerId, out var isLandData))
        {
            foreach (var item in req.MissionIds)
            {
                var mission = GameData.Instance.EventIslandBreakerMissionTable.Values
                    .Where(x => x.Id == item).FirstOrDefault();

                rewardlist.Add(mission.RewardId);

                isLandData.Missions[item].Rewarded = true;
                
            }

            ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist);

            response.Reward = ret;
            response.UpdatedMissions.AddRange(MiniGameHelper
                .ToProtoDict<int, NetMiniGameIslandBreakerMission, MiniGameIslandBreakerMission>(isLandData.Missions).Values); 
            
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}