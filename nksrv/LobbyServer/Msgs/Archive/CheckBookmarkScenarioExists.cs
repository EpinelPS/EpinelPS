using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Archive
{
    [PacketPath("/bookmark/scenario/exist")]
    public class CheckBookmarkScenarioExists : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqExistScenarioBookmark>();

            var response = new ResExistScenarioBookmark();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
