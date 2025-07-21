using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Client
{
    [PacketPath("/system/checkversion")]
    public class CheckClientVersion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCheckClientVersion req = await ReadData<ReqCheckClientVersion>();
            ResCheckClientVersion response = new()
            {
                Availability = ResCheckClientVersion.Types.Availability.None
            };

            await WriteDataAsync(response);
        }
    }
}
