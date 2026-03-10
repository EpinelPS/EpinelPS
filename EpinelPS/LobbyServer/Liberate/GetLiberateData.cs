using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate
{
    [PacketPath("/liberate/get")]
    public class GetLiberateData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetLiberateData req = await ReadData<ReqGetLiberateData>();
            User user = User;

            ResGetLiberateData response = new() { };

            // TODO

            await WriteDataAsync(response);
        }
    }
}
