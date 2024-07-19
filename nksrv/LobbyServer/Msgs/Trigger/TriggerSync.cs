using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Trigger
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
