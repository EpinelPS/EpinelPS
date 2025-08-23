using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;
using System.Linq;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/quest/stage/clear")]
    public class ClearFavoriteItemQuestStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearFavoriteItemQuestStage req = await ReadData<ReqClearFavoriteItemQuestStage>();
            User user = GetUser();
            ResClearFavoriteItemQuestStage response = new();

            FavoriteItemQuestStageRecord? stageData = GameData.Instance.GetFavoriteItemQuestStageData(req.StageId);
            if (stageData == null)
            {
                await WriteDataAsync(response);
                return;
            }

            FavoriteItemQuestRecord? questData = GameData.Instance.GetFavoriteItemQuestTableData(req.FavoriteItemQuestId);
            if (questData != null)
            {
                NetUserFavoriteItemQuestData? existingQuest = user.FavoriteItemQuests.FirstOrDefault(q => q.QuestId == req.FavoriteItemQuestId);
                if (existingQuest != null) existingQuest.Clear = true;
                else user.FavoriteItemQuests.Add(new NetUserFavoriteItemQuestData { QuestId = req.FavoriteItemQuestId, Clear = true });

                if (questData.next_quest_id > 0 && user.FavoriteItemQuests.All(q => q.QuestId != questData.next_quest_id))
                {
                    user.FavoriteItemQuests.Add(new NetUserFavoriteItemQuestData { QuestId = questData.next_quest_id, Clear = false, Received = false });
                }
            }

            string stageMapId = GameData.Instance.GetMapIdFromChapter(stageData.chapter_id, stageData.chapter_mod);
            if (!user.FieldInfoNew.ContainsKey(stageMapId))
            {
                user.FieldInfoNew.Add(stageMapId, new FieldInfoNew());
            }
            if (!user.FieldInfoNew[stageMapId].CompletedStages.Contains(req.StageId))
            {
                user.FieldInfoNew[stageMapId].CompletedStages.Add(req.StageId);
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

