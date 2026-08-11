using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/unlock")]
public class UnlockArchiveRecord : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqUnlockArchiveRecord req = await ReadData<ReqUnlockArchiveRecord>();
        User user = GetUser();
        ResUnlockArchiveRecord response = new();

        ArchiveRecordManagerRecord record = GameData.Instance.archiveRecordManagerTable.GetValueOrDefault(req.ArchiveRecordId)
            ?? throw new BadHttpRequestException($"Unknown archive record {req.ArchiveRecordId}", 400);

        if (!user.UnlockedArchiveRecordIds.Contains(record.Id))
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

            user.UnlockedArchiveRecordIds.Add(record.Id);
            JsonDb.Save();
        }

        await WriteDataAsync(response);
    }
}
