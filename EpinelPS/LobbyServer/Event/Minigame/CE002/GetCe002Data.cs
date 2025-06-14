using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.CE002
{
    [PacketPath("/event/minigame/ce002/get")]
    public class GetCe002Data : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMiniGameCe002Data>();
            var user = GetUser();

            var response = new ResGetMiniGameCe002Data
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
