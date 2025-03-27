using EpinelPS.Utils;
using EpinelPS.StaticInfo;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/get")]
    public class GetArchives : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArchiveRecord>();

            var response = new ResGetArchiveRecord();

            // Explicitly select IDs from the records
            var allIds = GameData.Instance.archiveRecordManagerTable.Values.Select(record => record.id).ToList();

            // Add the IDs to the response lists
            response.ArchiveRecordManagerList.AddRange(allIds);
            response.UnlockedArchiveRecordList.AddRange(allIds);

            // Get entries with record_type "EventQuest"
            var eventQuestRecords = GameData.Instance.archiveRecordManagerTable.Values.Where(record => record.record_type == "EventQuest").ToList();

            response.ArchiveEventQuest = new();
            response.ArchiveEventQuest.UnlockedArchiveRecordManagerEventQuestIdList.AddRange(eventQuestRecords.Select(record => record.id));
            // TODO more fields


            // TODO: allow unlocking
            await WriteDataAsync(response);
        }
    }
}
