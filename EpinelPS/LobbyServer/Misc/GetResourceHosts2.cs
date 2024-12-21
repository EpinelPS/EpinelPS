using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/resourcehosts2")]
    public class CheckClientVersion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetResourceHosts2>();

            var r = new ResGetResourceHosts2();
            r.BaseUrl = GameConfig.Root.ResourceBaseURL;
            r.Version = req.Version;

            await WriteDataAsync(r);
        }
    }
}
