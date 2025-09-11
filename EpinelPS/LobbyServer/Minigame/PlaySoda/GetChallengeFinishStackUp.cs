using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda
{
    [PacketPath("/arcade/play-soda/challenge/finish/stackup")]
    public class GetChallengeFinishStackUp : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqFinishArcadePlaySodaStackUpChallenge>();

            var user = GetUser();

            var challengeStageId = GameData.Instance.EventPlaySodaChallengeModeTable.First(i => i.Value.game_type == EventPlaySodaGameType.StackUp).Key;

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

            await WriteDataAsync(new ResFinishArcadePlaySodaStackUpChallenge());

            JsonDb.Save();
        }
    }
}
