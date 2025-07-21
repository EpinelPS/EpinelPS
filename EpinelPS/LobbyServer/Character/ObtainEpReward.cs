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
            ReqObtainAttractiveReward req = await ReadData<ReqObtainAttractiveReward>();
            ResObtainAttractiveReward response = new();
            User user = GetUser();

            // look up ID from name code and level
            KeyValuePair<int, AttractiveLevelRewardRecord> levelUpRecord = GameData.Instance.AttractiveLevelReward.Where(x => x.Value.attractive_level == req.Lv && x.Value.name_code == req.NameCode).FirstOrDefault();

            foreach (NetUserAttractiveData item in user.BondInfo)
            {
                if (item.NameCode == req.NameCode)
                {
                    if (!item.ObtainedRewardLevels.Contains(levelUpRecord.Value.id))
                    {
                        item.ObtainedRewardLevels.Add(levelUpRecord.Value.id);

                        RewardTableRecord reward = GameData.Instance.GetRewardTableEntry(levelUpRecord.Value.reward_id) ?? throw new Exception("failed to get reward");
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
