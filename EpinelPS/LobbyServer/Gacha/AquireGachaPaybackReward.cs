using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha;

/// <summary>
/// Handles the step up gacha pity rewards
/// </summary>
[GameRequest("/gacha/acquirepaybackreward")]
public class AquireGachaPaybackReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAcquireGachaPaybackReward req = await ReadData<ReqAcquireGachaPaybackReward>();

        ResAcquireGachaPaybackReward response = new();
        User user = GetUser();

        // Get the current reward step data
        GachaPaybackStepRecord_Raw step = GameData.Instance.GachaPaybackStepRecords.Where( 
                                      step =>  GameData.Instance.GachaPaybackRecords.Where(payback => payback.Value.GachaId == req.GachaId).Select(payback => payback.Key).FirstOrDefault() == step.Value.PaybackId &&
                                     step.Value.Step == req.Step).Select( step => step.Value).FirstOrDefault();

        if (step != null){
            RewardRecord reward = GameData.Instance.RewardDataRecords[step.RewardId];

            // Update the rewarded step list
            if (!user.GachaPaybackData[req.GachaId].RewardedStepList.Contains(req.Step))
                user.GachaPaybackData[req.GachaId].RewardedStepList.Add(req.Step);

            // Process the reward on the server
            // Send the reward back to the game client
            response.Reward = RewardUtils.RegisterRewardsForUser(GetUser(), reward);
           
            // Save
            JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
    
}
