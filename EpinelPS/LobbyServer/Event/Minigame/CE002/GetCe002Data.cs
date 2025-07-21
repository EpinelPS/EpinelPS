using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.CE002
{
    [PacketPath("/event/minigame/ce002/get")]
    public class GetCe002Data : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetMiniGameCe002Data req = await ReadData<ReqGetMiniGameCe002Data>();
            Database.User user = GetUser();

            ResGetMiniGameCe002Data response = new()
            {
                Data = new()
                {
                    Ce002Id = req.Ce002Id
                }
            };

            // TODO implement properly

            await WriteDataAsync(response);
        }
    }
}
