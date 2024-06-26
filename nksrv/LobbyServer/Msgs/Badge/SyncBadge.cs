using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Badge
{
    [PacketPath("/badge/sync")]
    public class SyncBadge : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSyncBadge>();

            var response = new ResSyncBadge();
            WriteData(response);
        }
    }
}
