namespace EpinelPS.LobbyServer.Sidestory;

[GameRequest("/eventfield/sidestory/recommendation/reddot")]
public class GetRecommendationRedDot : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSideStoryRecommendationReddotData req = await ReadData<ReqSideStoryRecommendationReddotData>();
        User user = GetUser();

        ResSideStoryRecommendationReddotData response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
