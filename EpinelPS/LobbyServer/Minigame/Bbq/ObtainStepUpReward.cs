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

        if (!user.BBQInfoData.StepUpRewarded.Contains(req.StepUpRewardId))
        {
            user.BBQInfoData.StepUpRewarded.Add(req.StepUpRewardId);

            response.Data = user.BBQInfoData.ToNet();

            NetRewardData ret = new NetRewardData();

            EventBBQTycoonStepUpRewardRecord? rewardid = GameData.Instance.EventBBQTycoonStepUpRewardTable.Values
                .Where(x => x.Id == req.StepUpRewardId).FirstOrDefault();
            ret = RewardUtils.RegisterRewardsForUser(user, rewardid.RewardId);
            response.Reward = ret;
        }
        user.AddTrigger(Trigger.EventBBQTycoonDailyRewardCheck, 1, req.StepUpRewardId);

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}