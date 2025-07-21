using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/messenger/get")]
    public class GetArchiveMessenger : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Read the request containing ArchiveMessengerGroupId
            ReqGetArchiveMessenger req = await ReadData<ReqGetArchiveMessenger>();
            int groupId = req.ArchiveMessengerGroupId;

            // Initialize the response object
            ResGetArchiveMessenger response = new();

            // Get the relevant data from ArchiveMessengerConditionTable
            GameData gameData = GameData.Instance;

            if (gameData.archiveMessengerConditionRecords.TryGetValue(groupId, out ArchiveMessengerConditionRecord? conditionRecord))
            {
                foreach (ArchiveMessengerConditionList condition in conditionRecord.archive_messenger_condition_list)
                {
                    // Add each condition as a NetArchiveMessage in the response
                    response.ArchiveMessageList.Add(new NetArchiveMessage
                    {
                        ConditionId = condition.condition_id,
                        MessageId = conditionRecord.tid // Correctly using tid as MessageId
                    });
                }
            }

            // Write the response back
            await WriteDataAsync(response);
        }
    }
}
