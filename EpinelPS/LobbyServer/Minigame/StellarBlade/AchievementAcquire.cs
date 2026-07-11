using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Event.Mission;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.StellarBlade;

[GameRequest("/arcade/stellar-blade/achievement/acquire")]
public class AchievementAcquire : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeAcquireStellarBladeAchievementReward req = await ReadData<ReqArcadeAcquireStellarBladeAchievementReward>();
        User user = GetUser();
        ResArcadeAcquireStellarBladeAchievementReward response = new();
        NetStellarBladeRewardData ret = new();

        if (user.StellarBladeDatas.TryGetValue(req.ArcadeManagerId, out var stellar))
        {
            List<int> list = [];

            foreach (var item in req.AchievementIdList)
            {
                EventSBMissionRecord_Raw? mission = GameData.Instance.EventSBMissionTable.Values
                    .Where(x => x.Id == item).FirstOrDefault();

                if (mission!= null)
                {
                    List<int> slist = [];
                    slist = MiniGameHelper.GetMissionReward(ref ret, ref stellar, user, mission);
                    list.AddRange(slist);

                    StellarBladeMissionData? missdate = stellar.MissionData.FirstOrDefault(x => x.MissionId == mission.Id);
                    missdate.IsReceived = true;
                }

            }

            ret.Reward = RewardUtils.RegisterRewardsForUserDou(user, list);

            response.RewardData = ret;
            var misslist = MiniGameHelper.ToProtoList<NetStellarBladeMissionData, StellarBladeMissionData>(stellar.MissionData);
            response.UpdatedAchievementMissionDataList.AddRange(misslist);
        }      

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}