using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/event-quest/unlock")]
public class UnlockArchiveEventQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUnlockArchiveEventQuest req = await ReadData<ReqUnlockArchiveEventQuest>();
        User user = GetUser();
        ResUnlockArchiveEventQuest response = new();

        ArchiveRecordManagerRecord record = GameData.Instance.archiveRecordManagerTable.GetValueOrDefault(req.ArchiveRecordManagerId)
            ?? throw new BadHttpRequestException($"Unknown archive event quest record {req.ArchiveRecordManagerId}", 400);

        if (!user.UnlockedArchiveEventQuestIds.Contains(record.Id))
        {
            if (GameConfig.Root.ArchiveUnlockAll != true)
            {
                DbItemData material = user.Items.FirstOrDefault(item => item.ItemType == record.UnlockTicketId)
                    ?? throw new BadHttpRequestException($"Missing archive unlock ticket {record.UnlockTicketId}", 400);

                List<NetUserItemData> updatedItems = [];
                if (!EquipmentUtils.DeductMaterials(material, record.UnlockTicketCount, user, updatedItems))
                    throw new BadHttpRequestException("Not enough archive unlock tickets", 400);

                response.UserItem = updatedItems[0];
                if (record.UnlockRewardId > 0)
                    response.Reward = RewardUtils.RegisterRewardsForUser(user, record.UnlockRewardId);
            }

            user.UnlockedArchiveEventQuestIds.Add(record.Id);
            JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
}
