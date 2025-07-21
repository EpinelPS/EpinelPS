using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/bookmark/scenario/exist")]
    public class CheckBookmarkScenarioExists : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqExistScenarioBookmark req = await ReadData<ReqExistScenarioBookmark>();

            ResExistScenarioBookmark response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
