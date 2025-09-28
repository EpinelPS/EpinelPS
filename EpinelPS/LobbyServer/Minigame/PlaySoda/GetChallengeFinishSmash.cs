using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda
{
    [PacketPath("/arcade/play-soda/challenge/finish/smash")]
    public class GetChallengeFinishSmash : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqFinishArcadePlaySodaSmashChallenge>();

            var user = GetUser();

            var challengeStageId = GameData.Instance.EventPlaySodaChallengeModeTable.First(i => i.Value.GameType == EventPlaySodaGameType.Smash).Key;

            var arcadePlaySodaInfo = user.ArcadePlaySodaInfoList.First(i => i.ChallengeStageId == challengeStageId);

            if (arcadePlaySodaInfo.UserRank < request.Score)
            {
                arcadePlaySodaInfo.UserRank = request.Score;
            }

            arcadePlaySodaInfo.AccumulatedScore += request.Score;

            var pointValues = GetChallengeObtainPointReward.GetPointValues(arcadePlaySodaInfo.ChallengeStageId);

            if (pointValues.Length > arcadePlaySodaInfo.LastRewardStep && arcadePlaySodaInfo.AccumulatedScore >= pointValues[arcadePlaySodaInfo.LastRewardStep])
            {
                arcadePlaySodaInfo.CanReceivePointReward = true;
            }

            await WriteDataAsync(new ResFinishArcadePlaySodaSmashChallenge());

            JsonDb.Save();
        }
    }
}
