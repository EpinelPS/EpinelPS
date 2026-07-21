using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Recycle;

[GameRequest("/outpost/RecycleRoom/LevelUpResearch")]
public class LevelUpResearch : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        /*
         * Req Contains:
         * Tid: int value, research tId
         * Items: int value, used items.
         */
        ReqRecycleLevelUpResearch req = await ReadData<ReqRecycleLevelUpResearch>();
        User user = GetUser();
        ResRecycleLevelUpResearch response = new();

        user.ResearchProgress.TryGetValue(req.Tid, out RecycleRoomResearchProgress? progress);

        // Check progress is null, null means research is not unlocked.
        if (progress != null)
        {
            AddProgressToResearch(response, user, progress, req);
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }

    private static void AddProgressToResearch(ResRecycleLevelUpResearch response, User user, RecycleRoomResearchProgress progress, ReqRecycleLevelUpResearch req)
    {
        GameData.Instance.RecycleResearchStats.TryGetValue(req.Tid, out RecycleResearchStatRecord? statRecord);
        if (statRecord is null)
            return;
        RecycleResearchLevelRecord? levelRecord = GameData.Instance.RecycleResearchLevels.Values.Where(e => e.RecycleType == statRecord.RecycleType && e.RecycleSubType == statRecord.RecycleSubType)
            .FirstOrDefault(e => e.RecycleLevel == progress.Level);
        if (levelRecord is null)
            return;

        if (statRecord.RecycleType == RecycleType.Personal) // main research
        {
            DbItemData? usedItem = user.Items.FirstOrDefault(e => e.ItemType == levelRecord.ItemId); // item_Id equals level-up item's tId.
            if (usedItem is null || usedItem.Count < levelRecord.ItemValue)
                return;

            usedItem.Count -= levelRecord.ItemValue;
            response.Items.Add(NetUtils.UserItemDataToNet(usedItem));

            progress.Level += 1;
            progress.Hp = statRecord.Hp * progress.Level;
            response.Recycle = new()
            {
                Tid = req.Tid,
                Lv = progress.Level,
            };
        }
        else if (statRecord.RecycleType == RecycleType.Class || statRecord.RecycleType == RecycleType.Corporation)
        {
            for (int i = 0; i < req.LevelUpCount; i++)
            {
                RecycleResearchLevelRecord? currentLevelRecord = GameData.Instance.RecycleResearchLevels.Values
                    .Where(e => e.RecycleType == statRecord.RecycleType && e.RecycleSubType == statRecord.RecycleSubType)
                    .FirstOrDefault(e => e.RecycleLevel == progress.Level);
                if (currentLevelRecord is null) break;

                DbItemData? usedItem = user.Items.FirstOrDefault(e => e.ItemType == currentLevelRecord.ItemId);
                if (usedItem is null || usedItem.Count < currentLevelRecord.ItemValue) break;

                usedItem.Count -= currentLevelRecord.ItemValue;
                response.Items.Add(NetUtils.UserItemDataToNet(usedItem));
                progress.Level += 1;
            }
            response.Recycle = new()
            {
                Tid = req.Tid,
                Lv = progress.Level,
            };
        }
        else
        {
            throw new Exception($"unknown recycle type {statRecord.RecycleType}");
        }
    }
}
