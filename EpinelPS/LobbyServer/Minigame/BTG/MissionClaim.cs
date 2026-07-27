using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BTG;

[GameRequest("/arcade/btg/mission/claim")]
public class MissionClaim : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClaimArcadeBtgMissionReward req = await ReadData<ReqClaimArcadeBtgMissionReward>();
        User user = GetUser();
        ResClaimArcadeBtgMissionReward response = new();

        NetRewardData ret = new NetRewardData();
        List<int> rewardlist = [];
        if (user.BtgData.TryGetValue(req.BtgId, out var btgData))
        {
            foreach (var item in req.MissionIdList)
            {
                if (btgData.MissionDatas.TryGetValue(item,out var value))
                {
                    value.IsReceived = true;
                }

                var rewardid = GameData.Instance.EventBTGMissionTable.Values
               .Where(x => x.Id == item).FirstOrDefault();

                rewardlist.Add(rewardid.RewardId);                
            }
           
            ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist);
            response.Reward = ret;

            var missionlist = MiniGameHelper.ToProtoDict<int, NetMiniGameBtgMissionData, MiniGameBtgMissionData>(btgData.MissionDatas);
            response.AchievementMissionDataList.AddRange(missionlist.Values);
        }

        
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}