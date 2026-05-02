using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Outpost.Recycle;

[GameRequest("/outpost/RecycleRoom/PersonalResearchLevelUp")]
public class PersonalResearchLevelUp : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqPersonalResearchRecycleLevelUp req = await ReadData<ReqPersonalResearchRecycleLevelUp>();
        ResPersonalResearchRecycleLevelUp response = new();
        User user = GetUser();

        const int personalResearchTid = 1001;
        RecycleRoomResearchProgress personalResearchProgress = user.ResearchProgress[personalResearchTid] ?? throw new Exception("PersonalRearch not found.");
        personalResearchProgress.Level += req.LevelUpCount;
        response.Recycle = new()
        {
            Tid = personalResearchTid,
            Lv = personalResearchProgress.Level,
            Exp = personalResearchProgress.Exp
        };

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
