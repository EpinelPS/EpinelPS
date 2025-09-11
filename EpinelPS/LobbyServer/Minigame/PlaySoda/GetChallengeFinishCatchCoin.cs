using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda
{
    [PacketPath("/arcade/play-soda/challenge/finish/catchcoin")]
    public class GetChallengeFinishCatchCoin : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqFinishArcadePlaySodaCatchCoinChallenge>();

            var user = GetUser();

            var challengeStageId = GameData.Instance.EventPlaySodaChallengeModeTable.First(i => i.Value.game_type == EventPlaySodaGameType.CatchCoin).Key;

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

            await WriteDataAsync(new ResFinishArcadePlaySodaCatchCoinChallenge());

            JsonDb.Save();
        }
    }
}
