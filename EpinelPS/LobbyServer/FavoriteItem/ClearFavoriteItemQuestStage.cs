using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.FavoriteItem;

[GameRequest("/favoriteitem/quest/stage/clear")]
public class ClearFavoriteItemQuestStage : LobbyMessage
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

            if (questData.NextQuestId > 0 && user.FavoriteItemQuests.All(q => q.QuestId != questData.NextQuestId))
            {
                user.FavoriteItemQuests.Add(new NetUserFavoriteItemQuestData { QuestId = questData.NextQuestId, Clear = false, Received = false });
            }
        }

        string stageMapId = GameData.Instance.GetMapIdFromChapter(stageData.ChapterId, stageData.ChapterMod);

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == stageMapId);

        if (field == null)
        {
            field = new FieldInfoNew
            {
                MapName = stageMapId
            };
            user.FieldInfo.Add(field);
        }

        field.CompletedStages.Add(req.StageId);

        await GameContext.SaveChangesAsync();

        await WriteDataAsync(response);
    }
}

