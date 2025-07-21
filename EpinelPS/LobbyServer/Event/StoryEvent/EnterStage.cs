using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/enterstage")]
    public class EnterEventStoryStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterEventStage req = await ReadData<ReqEnterEventStage>();
            User user = GetUser();
            ResEnterEventStage response = new();

            await WriteDataAsync(response);
        }
    }
}
