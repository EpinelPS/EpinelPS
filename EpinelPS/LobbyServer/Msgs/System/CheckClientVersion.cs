using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.System
{
    [PacketPath("/system/checkversion")]
    public class CheckClientVersion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckClientVersion>();
            var response = new ResCheckClientVersion();

            if (GameConfig.Root.GameMaxVer != req.Version)
            {
                response.Availability = Availability.Available;
            }
            else
            {
                response.Availability = Availability.None;
            }

            await WriteDataAsync(response);
        }
    }
}
