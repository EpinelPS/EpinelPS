using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Storyline
{
    [PacketPath("/storyline/get")]
    public class GetStoryline : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetStorylineData req = await ReadData<ReqGetStorylineData>();

            ResGetStorylineData response = new();
            User user = User;

            // TODO

            await WriteDataAsync(response);
        }
    }

}
