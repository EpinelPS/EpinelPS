using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory;

[PacketPath("/eventfield/sidestory/recommendation/reddot")]
public class GetRecommendationRedDot : LobbyMsgHandler
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
