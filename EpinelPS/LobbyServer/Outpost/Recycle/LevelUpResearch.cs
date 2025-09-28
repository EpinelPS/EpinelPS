using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Recycle
{
    [PacketPath("/outpost/RecycleRoom/LevelUpResearch")]
    public class LevelUpResearch : LobbyMsgHandler
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
            RecycleResearchLevelRecord? levelRecord = GameData.Instance.RecycleResearchLevels.Values.Where(e => e.RecycleType == statRecord.RecycleType)
                .FirstOrDefault(e => e.RecycleLevel == progress.Level);
            if (levelRecord is null)
                return;

            if (statRecord.RecycleType == RecycleType.Personal) // main research
            {
                ItemData? usedItem = user.Items.FirstOrDefault(e => e.ItemType == levelRecord.ItemId); // item_Id equals level-up item's tId.
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
            else if (statRecord.RecycleType == RecycleType.Class || statRecord.RecycleType == RecycleType.Corporation) // class research or corporation research
            {
                NetItemData netItem = req.Items.Single();
                ItemData? usedItem = user.Items.FirstOrDefault(e => e.ItemType == netItem.Tid);
                if (usedItem is null)
                    return;

                usedItem.Count -= netItem.Count;
                response.Items.Add(NetUtils.UserItemDataToNet(usedItem));
                (int newLevel, int newExp) = CalcCorpAndClassLevelUp(statRecord.RecycleType, netItem.Count, progress.Level, progress.Exp);
                progress.Level = newLevel;
                progress.Exp = newExp;
                response.Recycle = new()
                {
                    Tid = req.Tid,
                    Lv = newLevel,
                    Exp = newExp,
                };
            }
            else
            {
                throw new Exception($"unknown recycle type {statRecord.RecycleType}");
            }
        }

        // First: level, Second: exp
        private static (int, int) CalcCorpAndClassLevelUp(RecycleType researchType, int itemCount, int startLevel = 1, int startExp = 0)
        {
            // levelRecord.exp is required exp to level up.
            IEnumerable<RecycleResearchLevelRecord> levelRecords = GameData.Instance.RecycleResearchLevels.Values.Where(e => e.RecycleType == researchType && e.RecycleLevel > startLevel);

            foreach (RecycleResearchLevelRecord? record in levelRecords)
            {
                if (itemCount < record.ItemValue)
                {
                    startExp += itemCount;
                    break;
                }

                itemCount -= record.ItemValue - startExp;
                startLevel += 1;
                startExp = 0;
            }

            return (startLevel, startExp);
        }
    }
}
