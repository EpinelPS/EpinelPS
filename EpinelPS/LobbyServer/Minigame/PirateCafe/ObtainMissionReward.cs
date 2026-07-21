using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PirateCafe;

[GameRequest("/arcade/PirateCafe/obtainmissionreward")]
public class ObtainMissionReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainArcadePirateCafeMissionReward req = await ReadData<ReqObtainArcadePirateCafeMissionReward>();
        User user = GetUser();
        ResObtainArcadePirateCafeMissionReward response = new();

        NetRewardData ret = new NetRewardData();
        List<int> rewardlist = [];
        foreach (var item in req.MissionIdList)
        {
            var mission = GameData.Instance.EventPirateCafeMissionTable.Values
                .Where(x => x.Id == item).FirstOrDefault();
            rewardlist.Add(mission.RewardId);
        }

        ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist);
        user.PirateCafeData.MissionRewardedList.AddRange(req.MissionIdList);
        response.Reward = ret;
        response.Data = MiniGameHelper.ToProto<NetArcadePirateCafeData, ArcadePirateCafeData>(user.PirateCafeData);
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}