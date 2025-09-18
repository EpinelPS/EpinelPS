using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda
{
    [PacketPath("/arcade/mvg/obtainachievementmissionreward")]
    public class ReqObtainAchievementMissionReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqObtainArcadeMvgAchievementMissionReward>();

            var user = GetUser();

            List<NetRewardData> rewards = [];

            foreach (var missionId in request.MissionIds)
            {
                var mission = GameData.Instance.EventMvgMissionTable[missionId];
                user.ArcadeInTheMirrorData.AchievementMissions.First(m => m.MissionId == mission.id).IsReceived = true;

                var achievement_mission = GameData.Instance.EventMvgMissionTable.First(m => m.Key > mission.id && m.Value.condition_type == EventMVGMissionConditionType.ClearAchievement);
                user.ArcadeInTheMirrorData.AchievementMissions.First(m => m.MissionId == achievement_mission.Key).Progress++;

                rewards.Add(RewardUtils.RegisterRewardsForUser(user, mission.reward_id));
            }

            var response = new ResObtainArcadeMvgAchievementMissionReward() { Reward = NetUtils.MergeRewards(rewards, user) };
            response.Missions.AddRange(user.ArcadeInTheMirrorData.AchievementMissions);

            await WriteDataAsync(response);

            JsonDb.Save();
        }

    }
}
