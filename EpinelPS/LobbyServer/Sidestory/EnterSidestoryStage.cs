using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/stage/enter")]
    public class EnterSidestoryStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterSideStoryStage>();
            var user = GetUser();

            var response = new ResEnterSideStoryStage();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
