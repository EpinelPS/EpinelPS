using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.Dessertrush;

[GameRequest("/arcade/dessertrush/obtainstepupreward")]
public class ObtainStepUpReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainArcadeDessertRushStepUpReward req = await ReadData<ReqObtainArcadeDessertRushStepUpReward>();
        User user = GetUser();
        ResObtainArcadeDessertRushStepUpReward response = new();

        List<int>? list = user.DessertRushData.StepUpRewardedList.ToList();
        user.AddUnique(list, req.StepUpRewardId);

        user.DessertRushData.StepUpRewardedList.Clear();
        user.DessertRushData.StepUpRewardedList.AddRange(list);

        response.Data = MiniGameHelper.ToProto<NetArcadeDessertRushData, ArcadeDessertRushData>(user.DessertRushData);
        NetRewardData ret = new NetRewardData();

        EventDessertRushStepUpRewardRecord? rewardid = GameData.Instance.EventDessertRushStepUpRewardTable.Values
            .Where(x => x.Id == req.StepUpRewardId).FirstOrDefault();
        ret = RewardUtils.RegisterRewardsForUser(user, rewardid.RewardId);
        response.Reward = ret;
        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}