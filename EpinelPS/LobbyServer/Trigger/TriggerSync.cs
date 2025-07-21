using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Trigger
{
    [PacketPath("/trigger/sync")]
    public class TriggerSync : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSyncTrigger req = await ReadData<ReqSyncTrigger>();
            User user = GetUser();

            // This request is responsible for fetching a log for
            // daily, weekly, challenge mission completion.
            // This endpoint also returns the entire "history" for the account when 
            // Seq = 0, which the client does when it is started for the first time, or when 
            // the "Clear Cache" option is invoked. 
            // When Seq = 0, the server limits the responses to 2000 items,
            // and HasRemainData is set to true.
            // TODO: Is it necessary to store the entire account history each time a stage
            // is cleared, why does the official server do this?

            ResSyncTrigger response = new();
            Console.WriteLine("needs " + req.Seq);

            // Look for triggers past that amount
            Database.Trigger[] newTriggers = [.. user.Triggers.Where(x => x.Id > req.Seq)];

            // Return all triggers
            int triggerCount = 0;
            foreach (Database.Trigger? item in newTriggers)
            {
                triggerCount++;

                response.Triggers.Add(item.ToNet());

                if (triggerCount >= 2000)
                {
                    response.HasRemainData = true;
                    break;
                }
            }
            await WriteDataAsync(response);
        }
    }
}
