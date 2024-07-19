using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Sidestory
{
    [PacketPath("/sidestory/list")]
    public class ListSideStory : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListSideStory>();
            var user = GetUser();

            var response = new ResListSideStory();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
