using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/resourcehosts2")]
    public class CheckClientVersion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetResourceHosts2 req = await ReadData<ReqGetResourceHosts2>();

            ResGetResourceHosts2 r = new()
            {
                BaseUrl = GameConfig.Root.ResourceBaseURL,
                Version = req.Version
            };

            await WriteDataAsync(r);
        }
    }
}
