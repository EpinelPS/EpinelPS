using EpinelPS.Utils;
using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/attractive/obtainreward")]
    public class ObtainEpReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainAttractiveReward>();
            var response = new ResObtainAttractiveReward();
            var user = GetUser();

            // look up ID from name code and level
            var levelUpRecord = GameData.Instance.AttractiveLevelReward.Where(x => x.Value.attractive_level == req.Level && x.Value.name_code == req.NameCode).FirstOrDefault();

            foreach (var item in user.BondInfo)
            {
                if (item.NameCode == req.NameCode)
                {
                    if (!item.ObtainedRewardLevels.Contains(levelUpRecord.Value.id))
                    {
                        item.ObtainedRewardLevels.Add(levelUpRecord.Value.id);

                        var reward = GameData.Instance.GetRewardTableEntry(levelUpRecord.Value.reward_id) ?? throw new Exception("failed to get reward");
                        response.Reward = RewardUtils.RegisterRewardsForUser(user, reward);

                        JsonDb.Save();
                    }
                    break;
                }
            }

            await WriteDataAsync(response);
        }
    }
}
