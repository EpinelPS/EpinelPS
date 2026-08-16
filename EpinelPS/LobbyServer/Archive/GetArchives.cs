using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/get")]
public class GetArchives : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetArchiveRecord>();

        ResGetArchiveRecord response = new();
        List<ArchiveRecordManagerRecord> records = [.. GameData.Instance.archiveRecordManagerTable.Values];
        List<int> allIds = [.. records.Select(record => record.Id)];

        // This is the archive catalog, not the user's unlock state.
        response.ArchiveRecordManagerList.AddRange(allIds);

        User user = GetUser();
        if (GameConfig.Root.ArchiveUnlockAll == true)
        {
            response.UnlockedArchiveRecordList.AddRange(allIds);
        }
        else
        {
            response.UnlockedArchiveRecordList.AddRange(user.UnlockedArchiveRecordIds);
        }

        List<ArchiveRecordManagerRecord> eventQuestRecords = [.. records
            .Where(record => record.RecordType == ArchiveRecordType.EventQuest)];

        response.ArchiveEventQuest = new();
        if (GameConfig.Root.ArchiveUnlockAll == true)
        {
            response.ArchiveEventQuest.UnlockedArchiveRecordManagerEventQuestIdList
                .AddRange(eventQuestRecords.Select(record => record.Id));
        }
        else
        {
            response.ArchiveEventQuest.UnlockedArchiveRecordManagerEventQuestIdList
                .AddRange(user.UnlockedArchiveEventQuestIds);
        }

        await WriteDataAsync(response);
    }
}
