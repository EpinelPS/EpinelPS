using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Client
{
    [PacketPath("/system/checkversion")]
    public class CheckClientVersion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckClientVersion>();
            var response = new ResCheckClientVersion();
            response.Availability = Availability.None;

            await WriteDataAsync(response);
        }
    }
}
