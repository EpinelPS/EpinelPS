using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/bookmark/event/scenario/exist")]
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
