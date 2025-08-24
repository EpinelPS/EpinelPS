using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.FavoriteItem
{
    [PacketPath("/favoriteitem/quest/obtain")]
    public class ObtainFavoriteItemQuestReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainFavoriteItemQuestReward req = await ReadData<ReqObtainFavoriteItemQuestReward>();
            User user = GetUser();

            FavoriteItemQuestRecord? questData = GameData.Instance.GetFavoriteItemQuestTableData(req.QuestId);
            if (questData == null)
            {
                throw new BadHttpRequestException("Quest not found");
            }

            NetUserFavoriteItemQuestData? userQuest = user.FavoriteItemQuests.FirstOrDefault(q => q.QuestId == req.QuestId);
            if (userQuest == null || !userQuest.Clear || userQuest.Received)
            {
                throw new BadHttpRequestException("Quest not cleared or reward already received");
            }

            List<CharacterRecord> characterRecords = GameData.Instance.CharacterTable.Values.Where(c => c.name_code == questData.name_code).ToList();
            if (!characterRecords.Any())
            {
                throw new Exception($"Failed to find character record with name_code: {questData.name_code}");
            }

            HashSet<int> characterTids = characterRecords.Select(c => c.id).ToHashSet();
            CharacterModel? character = user.Characters.FirstOrDefault(c => characterTids.Contains(c.Tid));
            
            int characterCsn = 0;
            if (character != null)
            {
                characterCsn = character.Csn;
            }

            RewardTableRecord ? reward = GameData.Instance.GetRewardTableEntry(questData.reward_id);
            if (reward?.rewards == null || reward.rewards.Length == 0 || reward.rewards[0].reward_type != "FavoriteItem")
            {
                if (questData.reward_id > 0 && reward != null)
                {
                    NetRewardData rewardData = RewardUtils.RegisterRewardsForUser(user, reward);
                    ResObtainFavoriteItemQuestReward genericResponse = new ResObtainFavoriteItemQuestReward { UserReward = rewardData };
                    userQuest.Received = true;
                    JsonDb.Save();
                    await WriteDataAsync(genericResponse);
                    return;
                }
                throw new Exception("FavoriteItem reward data not found for quest");
            }
            int newItemTid = reward.rewards[0].reward_id;

            if (character != null)
            {
                NetUserFavoriteItemData? existingEquippedItem = user.FavoriteItems.FirstOrDefault(f => f.Csn == characterCsn);
                if (existingEquippedItem != null)
                {
                    user.FavoriteItems.Remove(existingEquippedItem);
                }
            }

            NetRewardData finalRewardData = RewardUtils.RegisterRewardsForUser(user, reward);
            
            if (character != null && finalRewardData.UserFavoriteItems.Count > 0)
            {
                var newFavoriteItem = user.FavoriteItems.LastOrDefault(f => f.Tid == newItemTid);
                if (newFavoriteItem != null)
                {
                    newFavoriteItem.Csn = characterCsn; // Equip item by setting Csn
                }
            }

            userQuest.Received = true;

            ResObtainFavoriteItemQuestReward response = new ResObtainFavoriteItemQuestReward { UserReward = finalRewardData };

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}