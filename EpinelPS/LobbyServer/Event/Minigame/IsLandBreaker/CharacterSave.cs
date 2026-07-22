using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/character/save")]
public class CharacterSave : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveMiniGameIslandBreakerLastSelectedCharacter req = await ReadData<ReqSaveMiniGameIslandBreakerLastSelectedCharacter>();
        User user = GetUser();
        ResSaveMiniGameIslandBreakerLastSelectedCharacter response = new();

        if (user.IsLandBreakerDatas.TryGetValue(req.IslandBreakerId, out var isLandData))
        {
            isLandData.CharacterStatistics.TryAdd(req.CharacterId, new MiniGameIslandBreakerCharacterStatistics()
            {
                CharacterId = req.CharacterId                
            });
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}