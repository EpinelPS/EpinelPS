using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/mission/acquire")]
public class MissionAcquire : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAcquireArcadeDragonDungeonRunMissionReward req = await ReadData<ReqAcquireArcadeDragonDungeonRunMissionReward>();
        User user = GetUser();
        ResAcquireArcadeDragonDungeonRunMissionReward response = new();

        NetRewardData ret = new();
        List<int> rewardlist = [];
        foreach (var item in req.MissionIdList)
        {
            user.DDRDatas.RewardedMissionIdList.AddUnique(item);
            EventDragonDungeonRunMissionRecord_Raw? reward = GameData.Instance.EventDragonDungeonRunMissionTable.Values
                .Where(x => x.Id == item).FirstOrDefault();

            rewardlist.Add(reward.RewardId);
        }
        ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist);

        response.Reward = ret;
        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}