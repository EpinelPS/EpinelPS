using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/view/set")]
    public class SetViewed : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetViewSideStory req = await ReadData<ReqSetViewSideStory>();
            User user = GetUser();

            ResSetViewSideStory response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
