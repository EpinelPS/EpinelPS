using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.Collections;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tls;

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
            // TODO: item_value has value, but idk what it is used for
            var levelRecord = GameData.Instance.RecycleResearchLevels.Values.Where(e => e.recycle_type == statRecord.recycle_type)
                .FirstOrDefault(e => e.recycle_level == progress.Level);
            if (levelRecord is null)
                return;

            if (statRecord.recycle_type == "Personal")
            {
                var usedItem = user.Items.FirstOrDefault(e => e.ItemType == levelRecord.item_id);
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
            else if (statRecord.recycle_type == "Class" || statRecord.recycle_type == "Corporation")
            {
                if (req.Items.Count <= 0)
                    return;

                var usedItem = user.Items.IntersectBy(req.Items.Select(e => e.Tid), first => first.ItemType).FirstOrDefault();
                if (usedItem is null || usedItem.Count < req.Items.Count)
                    return;

                int maxConsumableCount = levelRecord.exp - progress.Exp;
                if (req.Items.Count > maxConsumableCount)
                    return;

                if (req.Items.Count == maxConsumableCount)
                {
                    progress.Level += 1;
                    progress.Exp = 0;
                    progress.Attack = statRecord.attack * progress.Level;
                    progress.Defense = statRecord.defense * progress.Level;
                    progress.Hp = statRecord.hp * progress.Level;
                }
                else if (req.Items.Count < maxConsumableCount)
                {
                    progress.Exp += req.Items.Count;
                }

                response.Recycle = new()
                {
                    Tid = req.Tid,
                    Lv = progress.Level,
                    Exp = progress.Exp,
                };
            }
            else
            {
                throw new Exception($"unknown recycle type {statRecord.recycle_type}");
            }
        }
    }
}
