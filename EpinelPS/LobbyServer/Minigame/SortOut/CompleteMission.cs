using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.SortOut;

[GameRequest("/arcade/sortout/completemission")]
public class CompleteMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteArcadeSortOutMission req = await ReadData<ReqCompleteArcadeSortOutMission>();
        User user = GetUser();
        ResCompleteArcadeSortOutMission response = new();

        NetRewardData ret = new NetRewardData();
        List<int> rewardlist = [];
        foreach (var item in req.MissionIds)
        {
            EventSortOutMissionRecord_Raw? mission = GameData.Instance.EventSortOutMissionTable.Values
                .Where(x => x.Id == item).FirstOrDefault();
            rewardlist.Add(mission.RewardId);            
        }

        ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist);
        user.SortOutData.MissionRewarded.AddRangeUnique(req.MissionIds);
        response.Reward = ret;
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}