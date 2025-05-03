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
            var req = await ReadData<ReqGetArchiveMessenger>();
            var groupId = req.ArchiveMessengerGroupId;

            // Initialize the response object
            var response = new ResGetArchiveMessenger();
            
            // Get the relevant data from ArchiveMessengerConditionTable
            var gameData = GameData.Instance;

            if (gameData.archiveMessengerConditionRecords.TryGetValue(groupId, out var conditionRecord))
            {
                foreach (var condition in conditionRecord.archive_messenger_condition_list)
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
