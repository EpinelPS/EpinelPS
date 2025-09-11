using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda
{
    [PacketPath("/arcade/play-soda/challenge/obtain-point-reward")]
    public class GetChallengeObtainPointReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqObtainArcadePlaySodaPointReward>();

            var user = GetUser();

            var arcadePlaySodaInfo = user.ArcadePlaySodaInfoList.First(i => i.ChallengeStageId == request.ChallengeStageId);

            arcadePlaySodaInfo.CanReceivePointReward = false;

            List<NetRewardData> rewards = [];

            var pointValues = GetPointValues(arcadePlaySodaInfo.ChallengeStageId);

            while (pointValues.Length > arcadePlaySodaInfo.LastRewardStep && arcadePlaySodaInfo.AccumulatedScore >= pointValues[arcadePlaySodaInfo.LastRewardStep])
            {
                arcadePlaySodaInfo.LastRewardStep++;
                rewards.Add(RewardUtils.RegisterRewardsForUser(user, GameData.Instance.EventPlaySodaPointRewardTable.First(r => (int)r.Value.game_type == arcadePlaySodaInfo.ChallengeStageId && r.Value.step == arcadePlaySodaInfo.LastRewardStep && r.Value.point_value == pointValues[arcadePlaySodaInfo.LastRewardStep - 1]).Value.reward_id));
            }

            await WriteDataAsync(new ResObtainArcadePlaySodaPointReward() { LastRewardStep = arcadePlaySodaInfo.LastRewardStep, Reward = NetUtils.MergeRewards(rewards, user) });

            JsonDb.Save();
        }

        public static int[] GetPointValues(int challengeStageId)
        {
            var s = (int)EventPlaySodaGameType.Smash;
            if (challengeStageId == (int)EventPlaySodaGameType.CatchCoin)
            {
                return [300000, 600000, 1000000];
            }
            else if (challengeStageId == (int)EventPlaySodaGameType.Smash)
            {
                return [120000, 240000, 400000];
            }

            return [360000, 720000, 1200000];
        }
    }
}
