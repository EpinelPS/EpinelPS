using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Trigger
{
    [PacketPath("/trigger/sync")]
    public class TriggerSync : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = ReadData<ReqSyncTrigger>();

            var response = new ResSyncTrigger();
            await WriteDataAsync(response);
        }
    }
}
