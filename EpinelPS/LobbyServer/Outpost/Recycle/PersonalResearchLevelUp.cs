using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

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

        GameData.Instance.RecycleResearchStats.TryGetValue(personalResearchTid, out RecycleResearchStatRecord? statRecord);
        if (statRecord is null) return;

        for (int i = 0; i < req.LevelUpCount; i++)
        {
            RecycleResearchLevelRecord? levelRecord = GameData.Instance.RecycleResearchLevels.Values
                .Where(e => e.RecycleType == statRecord.RecycleType && e.RecycleSubType == statRecord.RecycleSubType)
                .FirstOrDefault(e => e.RecycleLevel == personalResearchProgress.Level);
            if (levelRecord is null) break;

            DbItemData? usedItem = user.Items.FirstOrDefault(e => e.ItemType == levelRecord.ItemId);
            if (usedItem is null || usedItem.Count < levelRecord.ItemValue) break;

            usedItem.Count -= levelRecord.ItemValue;
            response.Item = NetUtils.UserItemDataToNet(usedItem);
            personalResearchProgress.Level += 1;
            personalResearchProgress.Hp = statRecord.Hp * personalResearchProgress.Level;
        }

        response.Recycle = new()
        {
            Tid = personalResearchTid,
            Lv = personalResearchProgress.Level
        };

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
