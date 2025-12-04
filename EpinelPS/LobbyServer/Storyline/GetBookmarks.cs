using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Storyline
{
    [PacketPath("/storyline/bookmark/get")]
    public class GetBookmarks : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetStorylineBookmarks req = await ReadData<ReqGetStorylineBookmarks>();

            ResGetStorylineBookmarks response = new();
            User user = GetUser();

            // TODO

            await WriteDataAsync(response);
        }
    }

}
