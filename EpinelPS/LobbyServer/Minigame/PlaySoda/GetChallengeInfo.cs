using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda
{
    [PacketPath("/arcade/play-soda/challenge/info")]
    public class GetChallengeInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqGetArcadePlaySodaChallengeModeInfo>();

            var user = GetUser();

            ResGetArcadePlaySodaChallengeModeInfo response = new() { WholeUser = LobbyHandler.CreateWholeUserDataFromDbUser(user) };

            for (int i = 0; i < GameData.Instance.EventPlaySodaChallengeModeTable.Count; i++)
            {
                if (user.ArcadePlaySodaInfoList.Count < GameData.Instance.EventPlaySodaChallengeModeTable.Count)
                {
                    user.ArcadePlaySodaInfoList.Add(new() { AccumulatedScore = 0, CanReceivePointReward = false, ChallengeStageId = GameData.Instance.EventPlaySodaChallengeModeTable.Keys.ElementAt(i), LastRewardStep = 0, IsInGuild = false, UserRank = 0, UserMaxScoreInUnion = 0 /* TODO UNIONS */ });
                }

                response.ArcadePlaySodaEachGameInfoList.Add(user.ArcadePlaySodaInfoList[i]);
            }

            await WriteDataAsync(response);

            JsonDb.Save();
        }
    }
}
