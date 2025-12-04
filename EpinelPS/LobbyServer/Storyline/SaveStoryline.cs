using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Storyline
{
    [PacketPath("/storyline/save")]
    public class SaveStoryline : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSaveRecentStoryline req = await ReadData<ReqSaveRecentStoryline>();

            ResGetStorylineData response = new();
            User user = GetUser();

            // TODO

            await WriteDataAsync(response);
        }
    }

}
