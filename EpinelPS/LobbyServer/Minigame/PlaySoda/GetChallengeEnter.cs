using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.PlaySoda;

[GameRequest("/arcade/play-soda/challenge/enter")]
public class GetChallengeEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var request = await ReadData<ReqEnterArcadePlaySodaChallengeStage>();

        var user = GetUser();

        ResEnterArcadePlaySodaChallengeStage response = new()
        {
            UserMaxScore = user.ArcadePlaySodaInfoList.First(i => i.ChallengeStageId == request.ChallengeStageId).UserRank
        };

        await WriteDataAsync(response);

        JsonDb.Save();
    }
}
