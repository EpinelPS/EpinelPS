using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/seen/save")]
public class SeenSave : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveMiniGameIslandBreakerSeenContent req = await ReadData<ReqSaveMiniGameIslandBreakerSeenContent>();
        User user = GetUser();
        ResSaveMiniGameIslandBreakerSeenContent response = new();

        if (user.IsLandBreakerDatas.TryGetValue(req.IslandBreakerId, out var isLandData))
        {
            isLandData.SeenCharacterIds.AddRangeUnique(req.SeenCharacterIds);
            isLandData.SeenImageIds.AddRangeUnique(req.SeenImageIds);
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}