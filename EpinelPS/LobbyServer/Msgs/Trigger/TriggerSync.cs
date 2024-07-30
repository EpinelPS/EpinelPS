using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Trigger
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
