using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX;

[GameRequest("/event/minigame/azx/set/skill/unlocked")]
public class SetAzxSkillUnlocked : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // ReqSetMiniGameAzxSkillUnlocked Fields
        //  int AzxId
        //  int SkillId
        ReqSetMiniGameAzxSkillUnlocked req = await ReadData<ReqSetMiniGameAzxSkillUnlocked>();
        User user = GetUser();

        ResSetMiniGameAzxSkillUnlocked response = new();

        if (req.SkillId > 0 && req.AzxId > 0)
            AzxHelper.SetSkillUnlocked(user, req.AzxId, req.SkillId);

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}