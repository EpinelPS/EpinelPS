using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/stage/enter")]
    public class EnterSidestoryStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterSideStoryStage req = await ReadData<ReqEnterSideStoryStage>();
            Database.User user = GetUser();

            ResEnterSideStoryStage response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
