using EpinelPS.Data;
using EpinelPS.LobbyServer.Minigame;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/get/reddot/data")]
public class GetReddotData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameIslandBreakerRedDotData req = await ReadData<ReqGetMiniGameIslandBreakerRedDotData>();
        User user = GetUser();
        ResGetMiniGameIslandBreakerRedDotData response = new();

        if (user.IsLandBreakerDatas.TryGetValue(req.IslandBreakerId, out var isLandData))
        {
            if (MiniGameHelper.IsLandGetComplete(isLandData.Missions))
            {
                response.MissionRewardExists = true;
            }
        }

        //response.MissionRewardExists = false;
        // TODO
        await WriteDataAsync(response);
    }

   
}