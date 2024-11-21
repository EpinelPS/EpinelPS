using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/bookmark/event/scenario/exist")]
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
