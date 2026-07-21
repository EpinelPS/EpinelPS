using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.Bbq;

[GameRequest("/arcade/bbq/obtainstepupreward")]
public class ObtainStepUpReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainArcadeBBQStepUpReward req = await ReadData<ReqObtainArcadeBBQStepUpReward>();
        User user = GetUser();
        ResObtainArcadeBBQStepUpReward response = new();

        user.BBQInfoData.StepUpRewardedList.AddUnique(req.StepUpRewardId);        
        response.Data = MiniGameHelper.BBQToNet(user.BBQInfoData);
        NetRewardData ret = new NetRewardData();

        EventBBQTycoonStepUpRewardRecord? rewardid = GameData.Instance.EventBBQTycoonStepUpRewardTable.Values
            .Where(x => x.Id == req.StepUpRewardId).FirstOrDefault();
        ret = RewardUtils.RegisterRewardsForUser(user, rewardid.RewardId);
        response.Reward = ret;
        user.AddTrigger(Trigger.EventBBQTycoonDailyRewardCheck, 1, req.StepUpRewardId);

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}