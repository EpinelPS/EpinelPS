using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel;

[GameRequest("/character/counsel/progress/reward")]
public class ProgressReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainCounselProgressReward req = await ReadData<ReqObtainCounselProgressReward>();
        User user = GetUser();

        ResObtainCounselProgressReward response = new() { Reward = new NetRewardData() };

        NetUserAttractiveData? bond = user.BondInfo.FirstOrDefault(x => x.NameCode == req.NameCode);
        if (bond == null)
        {
            bond = new NetUserAttractiveData
            {
                NameCode = req.NameCode,
                Lv = 1,
                Exp = 0,
                CanCounselToday = true
            };
            user.BondInfo.Add(bond);
        }

        if (bond.CompleteRewardStatus != CounselDialogCompleteRewardStatus.Received)
        {
            bond.CompleteRewardStatus = CounselDialogCompleteRewardStatus.Received;

            if (GameData.Instance.AttractiveCounselCharacterTable.TryGetValue(req.NameCode, out AttractiveCounselCharacterRecord_Raw? charCounselRecord) &&
                charCounselRecord != null &&
                charCounselRecord.CollectRewardId > 0)
            {
                RewardRecord? rewardData = GameData.Instance.GetRewardTableEntry(charCounselRecord.CollectRewardId);
                if (rewardData != null)
                {
                    response.Reward = RewardUtils.RegisterRewardsForUser(user, rewardData);
                }
            }

            JsonDb.Save();
        }

        response.Reward ??= new NetRewardData();

        await WriteDataAsync(response);
    }
}
