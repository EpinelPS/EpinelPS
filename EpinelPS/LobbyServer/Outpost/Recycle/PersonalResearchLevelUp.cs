using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Recycle
{
    [PacketPath("/outpost/RecycleRoom/PersonalResearchLevelUp")]
    public class PersonalResearchLevelUp : LobbyMsgHandler
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
}
