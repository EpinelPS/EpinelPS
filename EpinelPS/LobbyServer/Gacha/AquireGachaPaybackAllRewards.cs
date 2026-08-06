using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha;

/// <summary>
/// Handles the step up gacha pity rewards (get all)
/// </summary>
[GameRequest("/gacha/acquirepaybackallreward")]
public class AquireGachaPaybackAllRewards : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAcquireGachaPaybackAllReward req = await ReadData<ReqAcquireGachaPaybackAllReward>();

        ResAcquireGachaPaybackAllReward response = new();
        User user = GetUser();

        // Make sure the payback banner exists on the user object. The button would probably not be enabled if not, but just to be safe.
        if (user.GachaPaybackData.ContainsKey(req.GachaId))
        {

            // Get the current reward step data
            List<GachaPaybackStepRecord_Raw> steps = GameData.Instance.GachaPaybackStepRecords.Where(
                                          step => GameData.Instance.GachaPaybackRecords.Where(payback => payback.Value.GachaId == req.GachaId).Select(payback => payback.Key).FirstOrDefault() == step.Value.PaybackId &&
                                          step.Value.GachaCount <= user.GachaPaybackData[req.GachaId].GachaCount && !user.GachaPaybackData[req.GachaId].RewardedStepList.Contains(step.Value.Step)).Select(step => step.Value).ToList();

            if (steps.Count > 0)
            {
                // Process the reward on the server
                // Send the reward back to the game client
                response.Reward = RewardUtils.RegisterRewardsForUserDou(GetUser(), steps.Select(step => step.RewardId).ToList());
                response.RewardedStepList.AddRange(steps.Select(step => step.Step).ToList());

                foreach(var step in steps)
                    if (!user.GachaPaybackData[req.GachaId].RewardedStepList.Contains(step.Step))
                        user.GachaPaybackData[req.GachaId].RewardedStepList.Add(step.Step);

                // Save
                JsonDb.Save();
            }
        }

        await WriteDataAsync(response);
    }

}
