using nksrv.Net;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.System
{
    [PacketPath("/system/checkversion")]
    public class CheckClientVersion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<CheckVersionRequest>();
            var response = new CheckVersionResponse();

            if (GameConfig.Root.GameMaxVer != req.Version)
            {
                response.VersionStatus = 1;
            }
            else
            {
                response.VersionStatus = 0;
            }

            await WriteDataAsync(response);
        }
    }
}
