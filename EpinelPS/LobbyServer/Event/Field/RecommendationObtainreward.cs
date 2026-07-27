using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Field;

[GameRequest("/eventfield/sidestory/recommendation/obtainreward")]
public class RecommendationObtainreward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainSideStoryRecommendationReward req = await ReadData<ReqObtainSideStoryRecommendationReward>();

        User user = GetUser();
        ResObtainSideStoryRecommendationReward response = new();

        // TODO
        await WriteDataAsync(response);
    }
}