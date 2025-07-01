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
             * Tid: int value, research tid
             * Items: int value, used items.
             */
            var req = await ReadData<ReqRecycleLevelUpResearch>();
            var user = GetUser();
            var response = new ResRecycleLevelUpResearch();

            user.ResearchProgress.TryGetValue(req.Tid, out var progress);

            // Check progress is null, null means research is not unlocked.
            if (progress != null)
            {
                AddProgressToResearch(response, user, progress, req);
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }

        private void AddProgressToResearch(ResRecycleLevelUpResearch response, User user, RecycleRoomResearchProgress progress, ReqRecycleLevelUpResearch req)
        {
            GameData.Instance.RecycleResearchStats.TryGetValue(req.Tid, out var statRecord);
            if (statRecord is null)
                return;
            var levelRecord = GameData.Instance.RecycleResearchLevels.Values.Where(e => e.recycle_type == statRecord.recycle_type)
                .FirstOrDefault(e => e.recycle_level == progress.Level);
            if (levelRecord is null)
                return;

            if (statRecord.recycle_type == "Personal") // main research
            {
                var usedItem = user.Items.FirstOrDefault(e => e.ItemType == levelRecord.item_id); // item_id equals level-up item's tid.
                if (usedItem is null || usedItem.Count < levelRecord.item_value)
                    return;

                usedItem.Count -= levelRecord.item_value;
                response.Items.Add(NetUtils.UserItemDataToNet(usedItem));

                progress.Level += 1;
                progress.Hp = statRecord.hp * progress.Level;
                response.Recycle = new()
                {
                    Tid = req.Tid,
                    Lv = progress.Level,
                };
            }
            else if (statRecord.recycle_type == "Class" || statRecord.recycle_type == "Corporation") // class research or corporation research
            {
                var netItem = req.Items.Single();
                var usedItem = user.Items.FirstOrDefault(e => e.ItemType == netItem.Tid);
                if (usedItem is null)
                    return;

                usedItem.Count -= netItem.Count;
                response.Items.Add(NetUtils.UserItemDataToNet(usedItem));
                (int newLevel, int newExp) = CalcCorpAndClassLevelUp(statRecord.recycle_type, netItem.Count, progress.Level, progress.Exp);
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
                throw new Exception($"unknown recycle type {statRecord.recycle_type}");
            }
        }

        // First: level, Second: exp
        private (int, int) CalcCorpAndClassLevelUp(string researchType, int itemCount, int startLevel = 1, int startExp = 0)
        {
            // levelRecord.exp is required exp to level up.
            var levelRecords = GameData.Instance.RecycleResearchLevels.Values.Where(e => e.recycle_type == researchType && e.recycle_level > startLevel);

            foreach (var record in levelRecords)
            {
                if (itemCount < record.exp)
                {
                    startExp += itemCount;
                    break;
                }

                itemCount -= record.exp - startExp;
                startLevel += 1;
                startExp = 0;
            }

            return (startLevel, startExp);
        }
    }
}
