using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.EventStory
{
    [PacketPath("/event/storydungeon/enterstage")]
    public class EnterEventStoryStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterEventStage>();
			var user = GetUser();
            var response = new ResEnterEventStage();

            await WriteDataAsync(response);
        }
    }
}
