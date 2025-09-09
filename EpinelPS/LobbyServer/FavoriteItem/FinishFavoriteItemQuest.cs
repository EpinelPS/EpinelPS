using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/quest/finish")]
    public class FinishFavoriteItemQuest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFinishFavoriteItemQuest req = await ReadData<ReqFinishFavoriteItemQuest>();
            User user = GetUser();

            FavoriteItemQuestRecord? questData = GetQuestDataFromGameData(req.FavoriteItemQuestId);
            if (questData == null)
            {
                ResFinishFavoriteItemQuest errorResponse = new();
                await WriteDataAsync(errorResponse);
                return;
            }


            NetUserFavoriteItemQuestData? existingQuest = user.FavoriteItemQuests.FirstOrDefault(q => q.QuestId == req.FavoriteItemQuestId);
   
            if (existingQuest != null)
            {
                if (existingQuest.Clear) 
                {
                    ResFinishFavoriteItemQuest errorResponse = new();
                    await WriteDataAsync(errorResponse);
                    return;
                }
                
                existingQuest.Clear = true;
            }
            else
            {
                NetUserFavoriteItemQuestData newQuest = new NetUserFavoriteItemQuestData
                {
                    QuestId = req.FavoriteItemQuestId,
                    Clear = true,
                    Received = false
                };
                user.FavoriteItemQuests.Add(newQuest);
            }
            
                        
            if (questData.next_quest_id > 0)
            {
                NetUserFavoriteItemQuestData? nextQuest = user.FavoriteItemQuests.FirstOrDefault(q => q.QuestId == questData.next_quest_id);
                if (nextQuest == null)
                {
                    NetUserFavoriteItemQuestData newQuest = new NetUserFavoriteItemQuestData
                    {
                        QuestId = questData.next_quest_id,
                        Clear = false,
                        Received = false
                    };
                    user.FavoriteItemQuests.Add(newQuest);
                }
              
            }
             
            JsonDb.Save();

            ResFinishFavoriteItemQuest response = new();
            await WriteDataAsync(response);
        }

        private FavoriteItemQuestRecord? GetQuestDataFromGameData(int questId)
        {
                if (GameData.Instance.FavoriteItemQuestTable.TryGetValue(questId, out FavoriteItemQuestRecord? questRecord))
                {
                    return questRecord;
                }

                return null;
           
        }

    }
}