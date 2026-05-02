using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX;

[GameRequest("/event/minigame/azx/set/character/unlocked")]
public class SetAzxCharacterUnlocked : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // ReqSetMiniGameAzxCharacterUnlocked Fields
        //  int AzxId
        //  int CharacterId
        ReqSetMiniGameAzxCharacterUnlocked req = await ReadData<ReqSetMiniGameAzxCharacterUnlocked>();
        User user = GetUser();

        ResSetMiniGameAzxCharacterUnlocked response = new();

        if (req.CharacterId > 0 && req.AzxId > 0)
            AzxHelper.SetCharacterUnlocked(user, req.AzxId, req.CharacterId);

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}