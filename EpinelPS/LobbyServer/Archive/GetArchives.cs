using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/get")]
    public class GetArchives : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetArchiveRecord req = await ReadData<ReqGetArchiveRecord>();

            ResGetArchiveRecord response = new();

            // Explicitly select IDs from the records
            List<int> allIds = [.. GameData.Instance.archiveRecordManagerTable.Values.Select(record => record.Id)];

            // Add the IDs to the response lists
            response.ArchiveRecordManagerList.AddRange(allIds);
            response.UnlockedArchiveRecordList.AddRange(allIds);

            // Get entries with record_type "EventQuest"
            List<ArchiveRecordManagerRecord> eventQuestRecords = [.. GameData.Instance.archiveRecordManagerTable.Values.Where(record => record.RecordType == ArchiveRecordType.EventQuest)];

            response.ArchiveEventQuest = new();
            response.ArchiveEventQuest.UnlockedArchiveRecordManagerEventQuestIdList.AddRange(eventQuestRecords.Select(record => record.Id));
            // TODO more fields


            // TODO: allow unlocking
            await WriteDataAsync(response);
        }
    }
}
